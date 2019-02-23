namespace JobProcessor
module Agent =

    open Message
    open System
    open System.Reflection
    open System.Linq

    let Queue = MailboxProcessor<IMessage>.Start(fun inbox ->
        let rec loop n =
            async {
                try
                    let! msg = inbox.Receive();

                    let t = msg.Assembly.GetType(msg.FullyQualifiedName)
                    let methodInfo = t.GetMethod(msg.MethodToRun, msg.MethodParametersTypes);
                    let o = Activator.CreateInstance(t, msg.ConstructorParameters);
                    let r = methodInfo.Invoke(o, msg.MethodParameters);

                    do! loop (n + 1)

                with
                | :? TimeoutException ->
                    printfn "The mailbox processor timed out."
                    do! loop (n + 1)
            }
        loop (0))

