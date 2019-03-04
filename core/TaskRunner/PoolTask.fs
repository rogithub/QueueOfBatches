namespace TaskRunner

open System
open System.Threading


type Result<'output> =
    | Error of exn
    | Cancel of OperationCanceledException
    | Success of 'output

type PoolTask<'input, 'output> = {
    Execute: Func<'input, CancellationToken,'output>;
    Complete: Action<'output>;
    Cancel: Action<OperationCanceledException>;
    Error: Action<exn>;
}

type PoolBatch<'output> = {
    Complete: Action<'output[]>;
    Cancel: Action<OperationCanceledException>;
    Error: Action<exn>;
}

