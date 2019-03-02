using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TaskRunner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasks;

namespace ServiceTest
{
	public class ServiceFactory
	{
		public ServiceFactory(Action<FinishResult> onJobCompleted, int pollInterval, int batchSize, Action<FinishResult, IAssemblyData> onRun = null, Action<FinishResult, IAssemblyData, Exception> onCancel = null, Action<FinishResult, IAssemblyData, Exception> onError = null)
		{
			this.Provider = new TaskProviderMock(null, onJobCompleted, null, null);
			this.PollInterval = pollInterval;
			this.BatchSize = batchSize;
			this.TokenSource = new CancellationTokenSource();
			var task = new RunAssemblyTaskMock(onRun, onCancel, onError);
			this.ServiceData = new Agent.InitData<IAssemblyData, FinishResult>(task, this.TokenSource.Token, this.Provider, this.PollInterval, this.BatchSize, this.InstanceId, this.InstanceName, this.Listener);
			this.Service = new Agent.Service<IAssemblyData, FinishResult>(this.ServiceData);
		}
		public Agent.InitData<IAssemblyData, FinishResult> ServiceData { get; }
		public Agent.Service<IAssemblyData, FinishResult> Service { get; }
		public CancellationTokenSource TokenSource { get; }
		public int PollInterval { get; set; }
		public int BatchSize { get; set; }
		public ConsoleTraceListener Listener = new ConsoleTraceListener();
		public Guid InstanceId = Guid.NewGuid();
		public string InstanceName = Environment.MachineName;
		public int DefaultA = 1;
		public int DefaultB = 2;
		public TaskProviderMock Provider { get; private set; }
		Func<int, int, int> sum = (a, b) => a + b;
		public static Action<FinishResult, IAssemblyData> onRunDefaultTests = (r, d) =>
		{
			Assert.IsNotNull(d);
			Assert.IsNull(r.Exception);
			Assert.AreEqual(FinishStatus.Succes, r.Status);
			Assert.AreEqual(3, r.Result);
			Assert.AreEqual(r.Id, d.Id);
		};

		public int GetDefaultExpectedResult()
		{
			return this.DefaultA + this.DefaultB;
		}

		public Guid[] AddTasks(int count, int itemsTimoutMs = -1)
		{
			IEnumerable<IAssemblyData> tasks = TaskFactory.Create(Enumerable.Repeat(sum, count), Enumerable.Repeat(new int[] { this.DefaultA, this.DefaultB }, count), itemsTimoutMs);
			this.Service.AddJobs(tasks);
			return (from t in tasks select t.Id).ToArray();
		}
	}
}
