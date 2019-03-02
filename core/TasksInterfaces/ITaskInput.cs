using System;

namespace TasksInterfaces
{
	public interface ITaskInput
	{
		Guid Id { get; }
		int TimeoutMilliseconds { get; }
	}
}
