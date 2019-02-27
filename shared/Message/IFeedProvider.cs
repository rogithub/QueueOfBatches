using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
	public interface IFeedProvider<T, A>
	{
		IEnumerable<T> GetNextBatch(int x);
		int StartBatch(IEnumerable<Guid> ids, string machineName, Guid instanceId);
		int AddJobs(T[] rows);
		int CompleteJob(A result);
	}
}
