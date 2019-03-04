namespace TaskRunner

open System
open System.Threading

type Task<'input, 'output> = {
    Execute: Func<'input, CancellationToken,'output>;
    Complete: Action<'output>;
    Cancel: Action<'input, Exception>;
    Error: Action<'input, Exception>;
}

