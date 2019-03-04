namespace TaskRunner
open System
open System.Threading
open System.Diagnostics


type Message<'input, 'output> = CancellationToken * 'input * AsyncReplyChannel<Result<'output>>
type InitData<'input, 'output> = {
    GlobalToken: CancellationToken;
    Listener: TraceListener; }

type Pool<'input, 'output> (queueToken: CancellationToken, task: PoolTask<'input, 'output>) =
    let GlobalToken = queueToken;
    let Task = task

    member private this.TaskRunner = MailboxProcessor<Message<'input, 'output>>.Start((fun inbox ->
        let rec loop() =
            async {
                let! (source, input, channel) = inbox.Receive();
                try
                    let result = Task.Execute.Invoke(input, source)
                    channel.Reply(Success result)
                    do! loop()
                with
                | :? OperationCanceledException as ex ->
                    Task.Cancel.Invoke(ex);
                    channel.Reply(Cancel ex)
                    do! loop()
                | ex ->
                    Task.Error.Invoke(ex);
                    channel.Reply(Error ex)
                    do! loop()
            }
        loop()), GlobalToken)

    member this.EnqueueTimeout input timeoutms =
        let _itSource = new CancellationTokenSource();
        let messageAsync = this.TaskRunner.PostAndAsyncReply((fun replyChannel -> _itSource.Token, input, replyChannel), timeoutms);
        Async.StartWithContinuations(messageAsync,
            (fun  success ->
                match success with
                | Success r -> Task.Complete.Invoke(r)
                | Error err -> Task.Error.Invoke(err)
                | Cancel can -> Task.Cancel.Invoke(can)
            ),
            (fun   error  -> Task.Error.Invoke(error) |> ignore),
            (fun     can  -> Task.Cancel.Invoke(can) |> ignore), _itSource.Token)
        messageAsync

    member this.Enqueue input =
        this.EnqueueTimeout input -1 // infinite timeout

    member this.EnqueueBatchTimeout list batch timeoutms =
        let items = list |> Seq.map(fun it -> this.EnqueueTimeout it timeoutms)
        let arr = Async.Parallel items
        Async.StartWithContinuations(arr,
            (fun  result -> batch.Complete.Invoke(result) |> ignore),
            (fun     err -> batch.Error.Invoke(err) |> ignore),
            (fun     can -> batch.Cancel.Invoke(can) |> ignore), GlobalToken)

    member this.EnqueueBatch list batch =
        this.EnqueueBatchTimeout list batch -1