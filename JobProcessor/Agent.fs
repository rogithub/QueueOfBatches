namespace JobProcessor
module Agent =

    open Message
    open System
    open System.Reflection

    type Message = IAssemblyData * AsyncReplyChannel<FinishResult>

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
                | ex ->
                    channel.Reply(new FinishResult(msg.MessageId, FinishStatus.Error, null, ex));
                    do! loop (n + 1)
            }
        loop (0))

    let private Process data callback =
        let messageAsync = AssemblyRunner.PostAndAsyncReply((fun replyChannel -> data, replyChannel), data.TimeoutMilliseconds);
        Async.StartWithContinuations(messageAsync,
            (fun  good -> callback good),
            (fun error -> callback (new FinishResult(data.MessageId, FinishStatus.Error, null, error))),
            (fun cancl -> callback (new FinishResult(data.MessageId, FinishStatus.Canceled, null, cancl))))


    let private FeedSource = MailboxProcessor<AsyncReplyChannel<_>>.Start(fun inbox ->
        let rec loop n =
            async {
                let! channel = inbox.Receive();

                let list = Seq.toArray <| DataBase.DbFeedProvider.GetNextBatch(DataBase.AppSettings.BatchSize);

                let ids = list |> Array.map (fun it -> it.MessageId)

                if (list.Length = 0) then
                    do! Async.Sleep DataBase.AppSettings.MillisecondsToBeIdle
                    printfn "round %d at %s" n (DateTime.Now.ToLongTimeString())
                else
                    printfn "round %d at %s processed %d" n (DateTime.Now.ToLongTimeString()) list.Length

                DataBase.DbFeedProvider.Start(ids) |> ignore;

                [for data in list do Process data (fun result ->
                        DataBase.DbFeedProvider.Update(result) |> ignore
                    )
                |> ignore] |> ignore

                channel.Reply()
                do! loop (n + 1);
            }
        loop (0))


    let rec Start () =
        let messageAsync = FeedSource.PostAndAsyncReply((fun replyChannel -> replyChannel));
        Async.StartWithContinuations(messageAsync,
            (fun _ -> Start()),
            (fun _ -> ()),
            (fun _ -> ()))

    let AddJobs jobs =
        let chunks = jobs |> Array.chunkBySize DataBase.AppSettings.BatchSize
        [for chunk in chunks do DataBase.DbFeedProvider.Save(chunk) |> ignore ] |> ignore
        chunks.Length