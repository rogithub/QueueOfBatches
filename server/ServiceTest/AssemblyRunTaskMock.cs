using Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest
{
	public class AssemblyRunTaskMock : AssemblyRunTask, IMessageTask<IAssemblyData, FinishResult>
	{
		private Action<FinishResult, IAssemblyData> OnSuccessExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnCancelExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnErrorExecuted;
		public AssemblyRunTaskMock(Action<FinishResult, IAssemblyData> onSuccess = null, Action<FinishResult, IAssemblyData, Exception> onCancel = null, Action<FinishResult, IAssemblyData, Exception> onError = null)
		{
			this.OnSuccessExecuted = onSuccess;
			this.OnCancelExecuted = onCancel;
			this.OnErrorExecuted = onError;
		}

		public new FinishResult OnRun(IAssemblyData input)
		{
			var result = base.OnRun(input);
			if (this.OnSuccessExecuted != null) this.OnSuccessExecuted(result, input);
			return result;
		}
		public new FinishResult OnCancell(IAssemblyData input, Exception ex)
		{
			var result = base.OnCancell(input, ex);
			if (this.OnCancelExecuted != null) this.OnCancelExecuted(result, input, ex);
			return result;
		}

		public new FinishResult OnError(IAssemblyData input, Exception ex)
		{
			var result = base.OnError(input, ex);
			if (this.OnErrorExecuted != null) this.OnErrorExecuted(result, input, ex);
			return result;
		}
	}
}
