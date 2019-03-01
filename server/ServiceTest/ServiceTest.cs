using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using JobProcessor;
using Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ServiceTest
{
	[TestClass]
	public class ServiceTest
	{
		[TestMethod]
		public void SuccessCase()
		{
			ConsoleTraceListener listener = new ConsoleTraceListener();
			Guid instanceId = Guid.NewGuid();
			int pollInterval = 0;
			int batchSize = 10;
			string instanceName = Environment.MachineName;
			var jobs = new ConcurrentDictionary<IAssemblyData, FinishResult>();
			int counter = 0;
			int itemsTimoutMs = -1;
			int tasksToCreate = 10000;
			int numberA = 1;
			int numberB = 2;
			int expectedResult = numberA + numberB;

			Action<FinishResult, IAssemblyData> onSuccess = (r, d) =>
			{
				Assert.IsNotNull(d);
				Assert.IsNull(r.Exception);
				Assert.AreEqual(FinishStatus.Succes, r.Status);
				Assert.AreEqual(expectedResult, r.Result);
				Assert.AreEqual(r.Id, d.Id);
			};

			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			Func<int, int, int> sum = (a, b) => a + b;

			ITaskProvider<IAssemblyData, FinishResult> provider = new TaskProviderMock(jobs, null, null, null, null);
			var c = new CancellationTokenSource();
			var task = new RunAssemblyTaskMock(onSuccess, onCancel, onError);
			var data = new Agent.InitData<IAssemblyData, FinishResult>(task, c.Token, provider, pollInterval, batchSize, instanceId, instanceName, listener);

			var watch = Stopwatch.StartNew();
			var service = new Agent.Service<IAssemblyData, FinishResult>(data);
			service.Start();

			IEnumerable<IAssemblyData> tasks = TaskFactory.Create(Enumerable.Repeat(sum, tasksToCreate), Enumerable.Repeat(new int[] { numberA, numberB }, tasksToCreate), itemsTimoutMs);
			service.AddJobs(tasks.ToArray());
			watch.Stop();

			var span = watch.Elapsed;
			Assert.IsTrue(span.TotalSeconds < 3);

			Assert.AreEqual(tasksToCreate, tasks.Count());
			Assert.AreEqual(tasksToCreate, jobs.Count());
			Assert.AreEqual(tasksToCreate, jobs.Select(j => j.Value.Status == FinishStatus.Succes).Count());
			Assert.AreEqual(0, counter);
		}
	}
}
