using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
	public enum FinishStatus
	{
		NotRun,
		RunToSucces,
		Error,
		TimedOut
	}

	public class FinishResult
	{
		public Guid MessageId { get; set; }
		public FinishStatus Status { get; set; }
		public object Result { get; set; }
		public Exception Exception { get; set; }
	}
}
