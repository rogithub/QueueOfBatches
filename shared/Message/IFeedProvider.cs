using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
	public interface IFeedProvider
	{
		IEnumerable<IAssemblyData> GetNextBatch(int x);
		int StartBatch(IEnumerable<Guid> rows, string machineName, Guid instanceId);
		int AddJobs(IAssemblyData[] rows);
		int CompleteJob(FinishResult result);
	}
}
