using Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest
{
	public class RunAssemblyTaskMock : RunAssemblyTask, ITask<IAssemblyData, FinishResult>
	{
		private Action<FinishResult, IAssemblyData> OnRunExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnCancelExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnErrorExecuted;
		public RunAssemblyTaskMock(Action<FinishResult, IAssemblyData> onRun = null, Action<FinishResult, IAssemblyData, Exception> onCancel = null, Action<FinishResult, IAssemblyData, Exception> onError = null)
		{
			this.OnRunExecuted = onRun;
			this.OnCancelExecuted = onCancel;
			this.OnErrorExecuted = onError;
		}

		public new FinishResult Run(IAssemblyData input)
		{
			var result = base.Run(input);
			if (this.OnRunExecuted != null) this.OnRunExecuted(result, input);
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
