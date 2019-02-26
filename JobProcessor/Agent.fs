namespace JobProcessor
module Agent =

    open Message
    open System
    open System.Reflection
    open System

    type Message = IAssemblyData * AsyncReplyChannel<FinishResult>

    type Starter = int * AsyncReplyChannel<int>


    let private AssemblyRunner = MailboxProcessor<Message>.Start(fun inbox ->
        let rec loop n =
            async {
                let! (msg, channel) = inbox.Receive();

                try

                    let t = msg.Assembly.GetType(msg.FullyQualifiedName)
                    let methodInfo = t.GetMethod(msg.MethodToRun, BindingFlags.Public ||| BindingFlags.Instance, null, CallingConventions.Any, msg.MethodParametersTypes, null);
                    let o = Activator.CreateInstance(t, msg.ConstructorParameters);
                    let r = methodInfo.Invoke(o, msg.MethodParameters);
                    channel.Reply(new FinishResult(msg.MessageId, FinishStatus.Succes, r, null));
                    do! loop (n + 1)

                with
                | :? TimeoutException ->
                    channel.Reply(new FinishResult(msg.MessageId, FinishStatus.TimedOut, null, null));
                    do! loop (n + 1)
                | ex ->
                    channel.Reply(new FinishResult(msg.MessageId, FinishStatus.Error, null, ex));
                    do! loop (n + 1)
            }
        loop (0))

    let private Process timeout data callback =
        let messageAsync = AssemblyRunner.PostAndAsyncReply((fun replyChannel -> data, replyChannel), timeout);
        Async.StartWithContinuations(messageAsync,
            (fun reply -> callback reply),
            (fun _ -> ()),
            (fun _ -> ()))

    let private FeedSource = MailboxProcessor<Starter>.Start(fun inbox ->
        let rec loop n =
            async {
                let! (timeToSleep, channel) = inbox.Receive();

                let list = Seq.toArray <| DataBase.DbFeedProvider.GetNextBatch();

                if (list.Length = 0 || AssemblyRunner.CurrentQueueLength > 0) then
                    do! Async.Sleep(timeToSleep)
                    printfn "round %d at %s" n (DateTime.Now.ToLongTimeString())
                else
                    printfn "round %d at %s processed %d" n (DateTime.Now.ToLongTimeString()) list.Length
                    [for data in list do Process -1 data (fun result -> DataBase.DbFeedProvider.Update(result) |> ignore)] |> ignore

                channel.Reply(timeToSleep);
                do! loop (n + 1);
            }
        loop (0))


    let rec Start timeToSleep =
        let messageAsync = FeedSource.PostAndAsyncReply((fun replyChannel -> timeToSleep, replyChannel));
        Async.StartWithContinuations(messageAsync,
            (fun reply -> Start(reply)),
            (fun _ -> ()),
            (fun _ -> ()))

    let AddJobs jobs =
        let count = DataBase.DbFeedProvider.Save(jobs);
        count