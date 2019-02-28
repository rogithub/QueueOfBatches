namespace JobProcessor
module Agent =

    open Message
    open System
    open System.Threading
    open System.Diagnostics
    open System.Diagnostics

    type Status =
        | NotStarted
        | Running
        | Stoped

    type Message<'input, 'output> = 'input * AsyncReplyChannel<'output>
    type InitData<'input, 'output> = {
        Task: IMessageTask<'input, 'output>;
        Token: CancellationToken;
        Provider: IFeedProvider<'input, 'output>;
        PollInterval: int;
        BatchSize: int;
        InstanceId: Guid;
        InstanceName: string;
        Listener: TraceListener; }

    type Service<'input, 'output> when 'input :> ITaskItem (initData: InitData<'input, 'output>) =
        let InitData = initData
        let mutable Status = NotStarted

        member private this.TaskRunner = MailboxProcessor<Message<'input, 'output>>.Start((fun inbox ->
            let rec loop() =
                async {
                    let! (msg, channel) = inbox.Receive();

                    try
                        channel.Reply(initData.Task.OnSuccess(msg));
                        do! loop()
                    with
                    | ex ->
                        channel.Reply(initData.Task.OnError(msg, ex));
                        do! loop()
                }
            loop()), InitData.Token)

        member private this.Process data =
            let messageAsync = this.TaskRunner.PostAndAsyncReply((fun replyChannel -> data, replyChannel), data.TimeoutMilliseconds);
            Async.StartWithContinuations(messageAsync,
                (fun  result -> InitData.Provider.CompleteJob(result) |> ignore),
                (fun   error -> InitData.Provider.CompleteJob(initData.Task.OnError(data, error)) |> ignore),
                (fun     can -> InitData.Provider.CompleteJob(initData.Task.OnCancell(data, can)) |> ignore), InitData.Token)


        member private this.FeedSource = MailboxProcessor<AsyncReplyChannel<_>>.Start((fun inbox ->
            let rec loop n =
                async {
                    let! channel = inbox.Receive();

                    let list = InitData.Provider.GetNextBatch(InitData.BatchSize);
                    let ids = list |> Seq.map(fun it -> it.Id)
                    let count = InitData.Provider.StartBatch(ids, InitData.InstanceName, InitData.InstanceId)

                    match count with
                    | 0 ->
                        do! Async.Sleep InitData.PollInterval
                        InitData.Listener.WriteLine(printf "round %d at %s" n (DateTime.Now.ToLongTimeString()))
                    | _ ->
                        InitData.Listener.WriteLine(printf "round %d at %s processed %d" n (DateTime.Now.ToLongTimeString()) count)

                    list |> Seq.iter(this.Process)

                    channel.Reply()
                    do! loop (n + 1)
                }
            loop (0)), InitData.Token)

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

        member this.AddJobs jobs =
            let chunks = jobs |> Array.chunkBySize InitData.BatchSize
            [for chunk in chunks do InitData.Provider.AddJobs(chunk) |> ignore ] |> ignore
            chunks.Length