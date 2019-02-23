namespace JobProcessor
module Agent =

    open Message
    open System

    let Queue = MailboxProcessor<IMessage>.Start(fun inbox ->
        let rec loop n =
            async {
                try
                    let! msg = inbox.Receive();

                    msg.OnExecute(msg.Param);

                    do! loop (n + 1)

                with
                | :? TimeoutException ->
                    printfn "The mailbox processor timed out."
                    do! loop (n + 1)
            }
        loop (0))

