using Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace ServiceTest
{
	public class TestQueue : ITaskQueue<IAssemblyData, FinishResult>
	{
		private Action<IEnumerable<IAssemblyData>> OnJobsAdded;
		private Action<FinishResult> OnJobCompleted;
		private Action<int> OnGetNextBatch;
		private Action<IEnumerable<Guid>, string, Guid> OnBatchStarted;

		public ConcurrentDictionary<Guid, FinishResult> Statuses { get; }

		public ConcurrentQueue<IEnumerable<IAssemblyData>> Batches { get; }
		public int BatchIndex { get; set; }

		public TestQueue(
			Action<IEnumerable<IAssemblyData>> onJobsAdded = null,
			Action<FinishResult> onJobCompleted = null,
			Action<int> onGetNextBatch = null,
			Action<IEnumerable<Guid>, string, Guid> onBatchStarted = null)
		{
			this.Batches = new ConcurrentQueue<IEnumerable<IAssemblyData>>();
			this.Statuses = new ConcurrentDictionary<Guid, FinishResult>();
			this.OnJobsAdded = onJobsAdded;
			this.OnJobCompleted = onJobCompleted;
			this.OnGetNextBatch = onGetNextBatch;
			this.OnBatchStarted = onBatchStarted;
		}

		public int Enqueue(IEnumerable<IAssemblyData> jobs)
		{
			this.Batches.Enqueue(jobs);

			if (this.OnJobsAdded != null) this.OnJobsAdded(jobs);
			return jobs.ToArray().Length;
		}

		public int CompleteTask(FinishResult result)
		{
			Func<Guid, FinishResult, FinishResult> fn = (key, v) => { return new FinishResult() { Id = result.Id, Status = result.Status }; };
			this.Statuses.AddOrUpdate(result.Id, new FinishResult() { Id = result.Id, Status = result.Status }, fn);

			if (this.OnJobCompleted != null) this.OnJobCompleted(result);
			return 1;
		}

		public IEnumerable<IAssemblyData> Dequeue(int size)
		{
			IEnumerable<IAssemblyData> data = new IAssemblyData[] { };

			if (this.Batches.TryDequeue(out data))
			{
				if (this.OnGetNextBatch != null) this.OnGetNextBatch(size);
				return data;
			}
			else
			{
				return new IAssemblyData[] { };
			}
		}

		public int Start(IEnumerable<Guid> ids, string machineName, Guid instanceId)
		{
			Func<Guid, FinishResult, FinishResult> fn = (key, v) => { return new FinishResult() { Id = v.Id, Status = FinishStatus.New }; };

			foreach (var id in ids)
			{
				this.Statuses.AddOrUpdate(id, new FinishResult() { Id = id, Status = FinishStatus.New }, fn);
			}

			if (this.OnBatchStarted != null) this.OnBatchStarted(ids, machineName, instanceId);
			return 1;
		}
	}
}
