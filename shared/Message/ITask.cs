using System;

namespace Message
{
	public interface ITask<input, output>
	{
		output OnSuccess(input input);
		output OnError(input input, Exception ex);
		output OnCancell(input input, Exception ex);
	}
}
