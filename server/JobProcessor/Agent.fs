namespace JobProcessor
module Agent =

    open Message
    open System
    open System.Reflection
    type Message = IAssemblyData * AsyncReplyChannel<FinishResult>

    type Service (token, instance: Guid) =
        let myToken = token
        member this.MachineName  = System.Environment.MachineName
        member this.InstanceId = instance

        member private this.AssemblyRunner = MailboxProcessor<Message>.Start((fun inbox ->
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
            loop (0)), myToken)

        member private this.Process data callback =
            let messageAsync = this.AssemblyRunner.PostAndAsyncReply((fun replyChannel -> data, replyChannel), data.TimeoutMilliseconds);
            Async.StartWithContinuations(messageAsync,
                (fun  good -> callback good),
                (fun error -> callback (new FinishResult(data.MessageId, FinishStatus.Error, null, error))),
                (fun _ -> callback (new FinishResult(data.MessageId, FinishStatus.Canceled, null, null))), myToken)


        member private this.FeedSource = MailboxProcessor<AsyncReplyChannel<_>>.Start((fun inbox ->
            let rec loop n =
                async {
                    let! channel = inbox.Receive();

                    let list = Seq.toArray <| DataBase.DbFeedProvider.GetNextBatch(DataBase.AppSettings.BatchSize);

                    match Array.length list with
                    | 0 ->
                        do! Async.Sleep DataBase.AppSettings.MillisecondsToBeIdle
                        printfn "round %d at %s" n (DateTime.Now.ToLongTimeString())
                    | _ ->
                        printfn "round %d at %s processed %d" n (DateTime.Now.ToLongTimeString()) list.Length

                    let ids = list |> Array.map(fun it -> it.MessageId)
                    DataBase.DbFeedProvider.Start(ids, this.MachineName, this.InstanceId) |> ignore

                    [for data in list do this.Process data (fun result -> DataBase.DbFeedProvider.Update(result) |> ignore )] |> ignore

                    channel.Reply()
                    do! loop (n + 1);
                }
            loop (0)), myToken)

        member this.Start () =
            let messageAsync = this.FeedSource.PostAndAsyncReply((fun replyChannel -> replyChannel));
            Async.StartWithContinuations(messageAsync,
                (fun _ -> this.Start()),
                (fun _ -> ()),
                (fun _ -> ()))

        member this.AddJobs jobs =
            let chunks = jobs |> Array.chunkBySize DataBase.AppSettings.BatchSize
            [for chunk in chunks do DataBase.DbFeedProvider.Save(chunk) |> ignore ] |> ignore
            chunks.Length