namespace JobProcessor
module Agent =

    open Message
    open System
    open System.Reflection
    open System.Drawing

    type Message = IAssemblyData * AsyncReplyChannel<FinishResult>
    type NextBatchInfo = { running: Guid[]; size: int; round: int }
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

    let private Process data callback =
        let messageAsync = AssemblyRunner.PostAndAsyncReply((fun replyChannel -> data, replyChannel), data.TimeoutMilliseconds);
        Async.StartWithContinuations(messageAsync,
            (fun reply -> callback reply),
            (fun _ -> ()),
            (fun _ -> ()))

    let private FeedSource = MailboxProcessor<Starter>.Start(fun inbox ->
        let rec loop info =
            async {
                let! (timeToSleep, channel) = inbox.Receive();

                let list = Seq.toArray <| DataBase.DbFeedProvider.GetNextBatch(info.size, info.running);
                let ids = list |> Array.map (fun it -> it.MessageId)

                if (list.Length = 0) then
                    do! Async.Sleep(timeToSleep)
                    printfn "round %d at %s" info.round (DateTime.Now.ToLongTimeString())
                else
                    printfn "round %d at %s processed %d" info.round (DateTime.Now.ToLongTimeString()) list.Length
                    [for data in list do Process data (fun result -> DataBase.DbFeedProvider.Update(result) |> ignore)] |> ignore

                channel.Reply(timeToSleep)
                do! loop ({ info with running = ids; size = (DataBase.AppSettings.BatchSize - AssemblyRunner.CurrentQueueLength); round = info.round + 1 });
            }
        loop ({ running = [||]; size = DataBase.AppSettings.BatchSize; round = 0 }))


    let rec Start timeToSleep =
        let messageAsync = FeedSource.PostAndAsyncReply((fun replyChannel -> timeToSleep, replyChannel));
        Async.StartWithContinuations(messageAsync,
            (fun reply -> Start(reply)),
            (fun _ -> ()),
            (fun _ -> ()))

    let AddJobs jobs =
        let count = DataBase.DbFeedProvider.Save(jobs);
        count