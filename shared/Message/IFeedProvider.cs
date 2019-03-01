using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
	public interface IFeedProvider<T, TResult>
	{
		IEnumerable<T> GetNextBatch(int size);
		int StartBatch(IEnumerable<Guid> ids, string machineName, Guid instanceId);
		int AddJobs(IEnumerable<T> jobs);
		int CompleteJob(TResult result);
	}
}
