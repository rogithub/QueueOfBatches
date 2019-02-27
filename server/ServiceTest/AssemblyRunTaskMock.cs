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
		private Action<FinishResult, IAssemblyData> OnRunExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnCancelExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnErrorExecuted;
		public AssemblyRunTaskMock(Action<FinishResult, IAssemblyData> onRun, Action<FinishResult, IAssemblyData, Exception> onCancel, Action<FinishResult, IAssemblyData, Exception> onError)
		{
			this.OnRunExecuted = onRun;
			this.OnCancelExecuted = onCancel;
			this.OnErrorExecuted = onError;
		}

		public new FinishResult OnRun(IAssemblyData input)
		{
			var result = base.OnRun(input);
			this.OnRunExecuted(result, input);
			return result;
		}
		public new FinishResult OnCancell(IAssemblyData input, Exception ex)
		{
			var result = base.OnCancell(input, ex);
			this.OnCancelExecuted(result, input, ex);
			return result;
		}

		public new FinishResult OnError(IAssemblyData input, Exception ex)
		{
			var result = base.OnError(input, ex);
			this.OnErrorExecuted(result, input, ex);
			return result;
		}
	}
}
