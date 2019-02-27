using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using JobProcessor;
using Message;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceTest
{
	[TestClass]
	public class ServiceTest
	{
		[TestMethod]
		public void InstaceTest()
		{
			Guid instanceId = Guid.NewGuid();
			int pollInterval = 0;
			int batchSize = 10;
			string instanceName = Environment.MachineName;
			var jobs = new Dictionary<IAssemblyData, FinishResult>();

			Action<IAssemblyData[]> onJobsAdded = (d) =>
			{
				Assert.IsNotNull(d);
			};
			Action<FinishResult> onJobCompleted = (d) =>
			{
				Assert.IsNotNull(d);
			};
			Action<int> onGetNextBatch = (size) =>
			{
				Assert.AreEqual(batchSize, size);
			};

			Action<IEnumerable<Guid>, string, Guid> onBatchStarted = (ids, name, id) =>
			{
				Assert.AreEqual(instanceName, name);
			};

			Action<FinishResult, IAssemblyData> onRunExecuted = (r, d) =>
			{
				Assert.AreEqual(FinishStatus.Succes, r.Result);
			};
			Action<FinishResult, IAssemblyData, Exception> onCancelExecuted = (r, d, e) =>
			{
				// never should be called
				Assert.IsTrue(false);
			};
			Action<FinishResult, IAssemblyData, Exception> onErrorExecuted = (r, d, e) =>
			{
				// never should be called
				Assert.IsTrue(false);
			};

			Func<int, int, int> sum = (a, b) =>
			{
				return a + b;
			};


			IFeedProvider<IAssemblyData, FinishResult> provider = new FeedProviderMock(jobs, onJobsAdded, onJobCompleted, onGetNextBatch, onBatchStarted);
			var c = new CancellationTokenSource();
			var task = new AssemblyRunTaskMock(onRunExecuted, onCancelExecuted, onErrorExecuted);
			var data = new Agent.InitData<IAssemblyData, FinishResult>(task, c.Token, provider, pollInterval, batchSize, instanceId, instanceName);
			var service = new Agent.Service<IAssemblyData, FinishResult>(data);



			IEnumerable<IAssemblyData> tasks = TaskFactory.Create(Enumerable.Repeat(sum, 10), Enumerable.Repeat(new int[] { 1, 2 }, 10), 10);
			service.AddJobs(tasks.ToArray());
			service.Start();


			Thread.Sleep(1000);
		}
	}
}
