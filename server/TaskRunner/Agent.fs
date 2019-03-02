namespace TaskRunner

module Agent =

    open Tasks
    open System
    open System.Threading
    open System.Diagnostics

    type Status =
        | NotStarted
        | Running
        | Stoped

    type Message<'input, 'output> = CancellationTokenSource * 'input * AsyncReplyChannel<'output>
    type InitData<'input, 'output> = {
        Task: ITask<'input, 'output>;
        GlobalToken: CancellationToken;
        Provider: ITaskQueue<'input, 'output>;
        PollInterval: int;
        BatchSize: int;
        InstanceId: Guid;
        InstanceName: string;
        Listener: TraceListener; }

    type Service<'input, 'output> when 'input :> ITaskInput (initData: InitData<'input, 'output>) =
        let InitData = initData
        let mutable Status = NotStarted

        member private this.TaskRunner = MailboxProcessor<Message<'input, 'output>>.Start((fun inbox ->
            let rec loop() =
                async {
                    let! (cancels, input, channel) = inbox.Receive();
                    try
                        channel.Reply(initData.Task.Run(input, cancels));
                        do! loop()
                    with
                    | ex ->
                        channel.Reply(initData.Task.OnError(input, ex));
                        do! loop()
                }
            loop()), InitData.GlobalToken)

        member private this.Process data =
            let _itSource = new CancellationTokenSource();
            let messageAsync = this.TaskRunner.PostAndAsyncReply((fun replyChannel -> _itSource, data, replyChannel), data.TimeoutMilliseconds);
            Async.StartWithContinuations(messageAsync,
                (fun  result -> InitData.Provider.CompleteTask(result) |> ignore),
                (fun   error -> InitData.Provider.CompleteTask(initData.Task.OnError(data, error)) |> ignore),
                (fun     can -> InitData.Provider.CompleteTask(initData.Task.OnCancell(data, can)) |> ignore), _itSource.Token)


        member private this.FeedSource = MailboxProcessor<AsyncReplyChannel<_>>.Start((fun inbox ->
            async {
                let! channel = inbox.Receive();

                let list = InitData.Provider.Dequeue(InitData.BatchSize);
                let ids = list |> Seq.map(fun it -> it.Id)
                let count = InitData.Provider.Start(ids, InitData.InstanceName, InitData.InstanceId)

                match count with
                | 0 ->
                    do! Async.Sleep InitData.PollInterval
                    InitData.Listener.WriteLine(printf "tick %s" (DateTime.Now.ToLongTimeString()))
                | _ ->
                    InitData.Listener.WriteLine(printf "tick %s processed %d" (DateTime.Now.ToLongTimeString()) count)

                list |> Seq.iter(this.Process)

                channel.Reply()

            }), InitData.GlobalToken)

        member private this.Starter () =
            let messageAsync = this.FeedSource.PostAndAsyncReply((fun replyChannel -> replyChannel));
            Async.StartWithContinuations(messageAsync,
                (fun _ ->
                    match Status with
                    | Running ->
                        this.Starter()
                    | _ ->
                        InitData.Listener.WriteLine(printf "[%s] Stoped " (DateTime.Now.ToLongTimeString()))
                ),
                (fun ex ->
                    this.Stop()
                    InitData.Listener.Fail("Error", ex.ToString())
                ),
                (fun ex ->
                    this.Stop()
                    InitData.Listener.Fail("Cancelled", ex.ToString())
                ))

        member this.Start () =
            match Status with
            | NotStarted | Stoped ->
                Status <- Running
                InitData.Listener.WriteLine(printf "[%s] Starting " (DateTime.Now.ToLongTimeString()))
                this.Starter()
            | _ -> () // do nothing

        member this.Stop () =
            match Status with
            | Running ->
                Status <- Stoped
                InitData.Listener.WriteLine(printf "[%s] Stoping " (DateTime.Now.ToLongTimeString()))
            | _ -> () // do nothing

        member this.Enqueue jobs =
            InitData.Provider.Enqueue(jobs)
