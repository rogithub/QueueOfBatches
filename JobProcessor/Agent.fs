namespace JobProcessor
module Agent =

    open Message
    open System
    open System.Reflection

    type Message = IAssemblyData * AsyncReplyChannel<FinishResult>

    let AssemblyRunner = MailboxProcessor<Message>.Start(fun inbox ->
        let rec loop n =
            async {
                let! (msg, channel) = inbox.Receive();

                try

                    let t = msg.Assembly.GetType(msg.FullyQualifiedName)
                    let methodInfo = t.GetMethod(msg.MethodToRun, BindingFlags.Public ||| BindingFlags.Instance, null, CallingConventions.Any, msg.MethodParametersTypes, null);
                    let o = Activator.CreateInstance(t, msg.ConstructorParameters);
                    let r = methodInfo.Invoke(o, msg.MethodParameters);
                    channel.Reply(new FinishResult(msg.MessageId, FinishStatus.RunToSucces, r, null));
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

    let Process timeout data callback =
        let messageAsync = AssemblyRunner.PostAndAsyncReply((fun replyChannel -> data, replyChannel), timeout);
        Async.StartWithContinuations(messageAsync,
            (fun reply -> callback reply),
            (fun _ -> ()),
            (fun _ -> ()))