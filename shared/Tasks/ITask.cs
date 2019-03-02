using System;
using System.Threading;

namespace Tasks
{
	public interface ITask<input, output>
	{
		output Run(input input, CancellationToken token);
		output OnError(input input, Exception ex);
		output OnCancell(input input, Exception ex);
	}
}
