using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using TaskRunner;
using Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ServiceTest
{
	[TestClass]
	public class ServiceTests
	{
		[TestMethod]
		public void Success()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10000;

			Action<FinishResult> onJobCompleted = (r) =>
			{
				Assert.AreEqual(r.Status, FinishStatus.Succes);
				Interlocked.Increment(ref counter);
			};
			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			ServiceFactory factory = new ServiceFactory(onJobCompleted, pollInterval, batchSize, ServiceFactory.onSuccessDefaultTests, onCancel, onError);

			factory.Service.Start();
			Guid[] tasksCreated = factory.AddTasks(tasksToCreate);
			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Thread.Sleep(1000 * 10); //wait all
			Assert.AreEqual(tasksToCreate, counter);
		}

		[TestMethod]
		public void StartStop()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10000;

			Action<FinishResult> onJobCompleted = (r) =>
			{
				Assert.AreEqual(r.Status, FinishStatus.Succes);
				Interlocked.Increment(ref counter);
			};
			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);

			ServiceFactory factory = new ServiceFactory(onJobCompleted, pollInterval, batchSize, ServiceFactory.onSuccessDefaultTests, onCancel, onError);

			// Start for the first time
			factory.Service.Start();

			factory.AddTasks(5000);
			// Stoping here
			factory.Service.Stop();
			factory.AddTasks(5000);
			// Start again
			factory.Service.Start();

			Thread.Sleep(1000 * 10); //wait all
			Assert.AreEqual(tasksToCreate, counter);
		}

		[TestMethod]
		public void NotTouchedIFCanceledBeforeStart()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10000;

			Action<FinishResult> onJobCompleted = (r) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData> onSuccess = (r, d) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			ServiceFactory factory = new ServiceFactory(onJobCompleted, pollInterval, batchSize, onSuccess, onCancel, onError);

			factory.TokenSource.Cancel();

			factory.Service.Start();

			Guid[] tasksCreated = factory.AddTasks(tasksToCreate);

			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Assert.AreEqual(0, counter);
		}

		[TestMethod]
		public void CanceledAfterStart()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10000;
			int tasksToCreate = 10000;

			Action<FinishResult> onJobCompleted = null;
			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) =>
			{
				Assert.IsNotNull(e);
				Assert.IsNotNull(f.Exception);
				Assert.AreEqual(FinishStatus.Canceled, f.Status);
				Assert.IsNull(f.Result);
				Assert.AreEqual(d.Id, f.Id);
				Interlocked.Increment(ref counter);
			};
			Action<FinishResult, IAssemblyData> onSuccess = (r, d) =>
			{
				Thread.Sleep(1000 * 60 * 60); //wait one hour for cancelation
			};
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			ServiceFactory factory = new ServiceFactory(onJobCompleted, pollInterval, batchSize, onSuccess, onCancel, onError);

			factory.Service.Start();

			Guid[] tasksCreated = factory.AddTasks(tasksToCreate);

			Thread.Sleep(1000); //wait for batch to start
			factory.TokenSource.Cancel();

			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Assert.AreEqual(tasksToCreate, counter);
		}
	}
}
