using System;

namespace Tasks
{
	public interface ITaskInput
	{
		Guid Id { get; }
		int TimeoutMilliseconds { get; }
	}
}
