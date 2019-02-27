using System;

namespace Message
{
	public class FinishResult
	{
		public FinishResult() { }
		public FinishResult(Guid id, FinishStatus status, object result, Exception ex)
		{
			this.Id = id;
			this.Status = status;
			this.Result = result;
			this.Exception = ex;
		}
		public Guid Id { get; set; }
		public FinishStatus Status { get; set; }
		public object Result { get; set; }
		public Exception Exception { get; set; }
	}
}
