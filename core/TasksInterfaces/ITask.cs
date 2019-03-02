using System;
using System.Threading;

namespace TasksInterfaces
{
	public interface ITask<input, output>
	{
		output Run(input input, CancellationTokenSource source);
		output OnError(input input, Exception ex);
		output OnCancell(input input, Exception ex);
	}
}
