using System;
using System.Threading;
using TasksInterfaces;

namespace ServiceTest
{
	public class AssemblyTaskMock : AssemblyTask, ITask<IAssemblyData, FinishResult>
	{
		private Action<FinishResult, IAssemblyData, CancellationTokenSource> OnRunExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnCancelExecuted;
		private Action<FinishResult, IAssemblyData, Exception> OnErrorExecuted;
		public AssemblyTaskMock(Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = null, Action<FinishResult, IAssemblyData, Exception> onCancel = null, Action<FinishResult, IAssemblyData, Exception> onError = null)
		{
			this.OnRunExecuted = onRun;
			this.OnCancelExecuted = onCancel;
			this.OnErrorExecuted = onError;
		}

		public new FinishResult Run(IAssemblyData input, CancellationTokenSource source)
		{
			var result = base.Run(input, source);
			if (this.OnRunExecuted != null) this.OnRunExecuted(result, input, source);
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
