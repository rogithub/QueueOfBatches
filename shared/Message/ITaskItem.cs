using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
	public interface ITaskItem
	{
		Guid Id { get; }
		int TimeoutMilliseconds { get; }
	}
}
