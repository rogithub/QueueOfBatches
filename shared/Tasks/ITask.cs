using System;

namespace Tasks
{
	public interface ITask<input, output>
	{
		output Run(input input);
		output OnError(input input, Exception ex);
		output OnCancell(input input, Exception ex);
	}
}
