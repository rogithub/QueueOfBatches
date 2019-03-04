namespace TaskRunner
open System
open System.Threading
open System.Diagnostics

type Message<'input, 'output> = CancellationToken * 'input * AsyncReplyChannel<'output>
type InitData<'input, 'output> = {
    GlobalToken: CancellationToken;
    Listener: TraceListener; }

type Pool<'input, 'output> (queueToken: CancellationToken, task: Task<'input, 'output>) =
    let GlobalToken = queueToken;
    let Task = task

    member private this.TaskRunner = MailboxProcessor<Message<'input, 'output>>.Start((fun inbox ->
        let rec loop() =
            async {
                let! (source, input, channel) = inbox.Receive();
                try
                    channel.Reply(Task.Execute.Invoke(input, source));
                    do! loop()
                with
                | ex ->
                    Task.Error.Invoke(input, ex);
                    do! loop()
            }
        loop()), GlobalToken)

    member this.Enqueue input timeoutms =
        let _itSource = new CancellationTokenSource();
        let messageAsync = this.TaskRunner.PostAndAsyncReply((fun replyChannel -> _itSource.Token, input, replyChannel), timeoutms);
        Async.StartWithContinuations(messageAsync,
            (fun  result -> Task.Complete.Invoke(result) |> ignore),
            (fun   error -> Task.Error.Invoke(input, error) |> ignore),
            (fun     can -> Task.Cancel.Invoke(input, can) |> ignore), _itSource.Token)