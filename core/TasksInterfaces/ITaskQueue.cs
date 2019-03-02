using System;
using System.Collections.Generic;

namespace TasksInterfaces
{
	public interface ITaskQueueClient<T>
	{
		int Enqueue(IEnumerable<T> jobs);
	}
	public interface ITaskQueue<T, TResult> : ITaskQueueClient<T>
	{
		IEnumerable<T> Dequeue(int batchSize);
		int Start(IEnumerable<Guid> ids, string machineName, Guid instanceId);
		int CompleteTask(TResult result);
	}
}
