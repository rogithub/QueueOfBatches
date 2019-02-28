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
	public class SumsTest
	{
		[TestMethod]
		public void SuccessCase()
		{
			Guid instanceId = Guid.NewGuid();
			int pollInterval = 0;
			int batchSize = 10;
			string instanceName = Environment.MachineName;
			var jobs = new ConcurrentDictionary<IAssemblyData, FinishResult>();
			int counter = 0;
			int itemsTimoutMs = -1;
			int tasksToCreate = 1000;
			int timeToWaitForAllToCompleteMs = 1000;
			int numberA = 1;
			int numberB = 2;
			int expectedResult = numberA + numberB;

			Action<FinishResult, IAssemblyData> onSuccess = (r, d) =>
			{
				Assert.AreEqual(expectedResult, r.Result);
				Assert.AreEqual(r.Id, d.Id);
			};

			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) =>
			{
				Interlocked.Increment(ref counter);
			};
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) =>
			{
				Interlocked.Increment(ref counter);
			};

			Func<int, int, int> sum = (a, b) =>
			{
				return a + b;
			};


			IFeedProvider<IAssemblyData, FinishResult> provider = new FeedProviderMock(jobs, null, null, null, null);
			var c = new CancellationTokenSource();
			var task = new AssemblyRunTaskMock(onSuccess, onCancel, onError);
			var data = new Agent.InitData<IAssemblyData, FinishResult>(task, c.Token, provider, pollInterval, batchSize, instanceId, instanceName);
			var service = new Agent.Service<IAssemblyData, FinishResult>(data);
			service.Start();

			IEnumerable<IAssemblyData> tasks = TaskFactory.Create(Enumerable.Repeat(sum, tasksToCreate), Enumerable.Repeat(new int[] { numberA, numberB }, tasksToCreate), itemsTimoutMs);
			service.AddJobs(tasks.ToArray());

			// wait one second to finish
			Thread.Sleep(timeToWaitForAllToCompleteMs);
			Assert.AreEqual(tasksToCreate, tasks.Count());
			Assert.AreEqual(0, counter);
		}
	}
}
