using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks
{
	public interface ITaskProvider<T, TResult>
	{
		IEnumerable<T> GetNextBatch(int size);
		int StartBatch(IEnumerable<Guid> ids, string machineName, Guid instanceId);
		int AddTasks(IEnumerable<T> jobs);
		int CompleteTask(TResult result);
	}
}
