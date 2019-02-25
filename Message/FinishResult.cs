using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
	public class FinishResult
	{
		public FinishResult() { }
		public FinishResult(Guid id, FinishStatus status, object result, Exception ex)
		{
			this.MessageId = id;
			this.Status = status;
			this.Result = result;
			this.Exception = ex;
		}
		public Guid MessageId { get; set; }
		public FinishStatus Status { get; set; }
		public object Result { get; set; }
		public Exception Exception { get; set; }
	}
}
