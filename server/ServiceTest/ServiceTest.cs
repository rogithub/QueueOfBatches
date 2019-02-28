using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using JobProcessor;
using Message;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;

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
			var jobs = new ConcurrentDictionary<IAssemblyData, FinishResult>();
			int counter = 0;

			Action<FinishResult, IAssemblyData> onSuccess = (r, d) =>
			{
				Assert.AreEqual(3, r.Result);
				Assert.AreEqual(r.Id, d.Id);
				System.Threading.Interlocked.Increment(ref counter);
			};

			Func<int, int, int> sum = (a, b) =>
			{
				return a + b;
			};


			IFeedProvider<IAssemblyData, FinishResult> provider = new FeedProviderMock(jobs, null, null, null, null);
			var c = new CancellationTokenSource();
			var task = new AssemblyRunTaskMock(onSuccess, null, null);
			var data = new Agent.InitData<IAssemblyData, FinishResult>(task, c.Token, provider, pollInterval, batchSize, instanceId, instanceName);
			var service = new Agent.Service<IAssemblyData, FinishResult>(data);



			IEnumerable<IAssemblyData> tasks = TaskFactory.Create(Enumerable.Repeat(sum, 10), Enumerable.Repeat(new int[] { 1, 2 }, 10), 10);
			service.AddJobs(tasks.ToArray());
			service.Start();

			// wait one second to finish
			Thread.Sleep(1000);

			int finalCount = tasks.Count();
			Assert.IsTrue(finalCount > 0);
			Assert.AreEqual(finalCount, counter);
		}
	}
}
