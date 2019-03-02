using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks
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
