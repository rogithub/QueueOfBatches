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
		int Start(Guid[] rows, string machineName, Guid instanceId);
		int Save(IAssemblyData[] rows);
		int Update(FinishResult result);
	}
}
