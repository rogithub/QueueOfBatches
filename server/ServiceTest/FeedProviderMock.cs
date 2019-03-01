using Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ServiceTest
{
	public class FeedProviderMock : IFeedProvider<IAssemblyData, FinishResult>
	{
		private Action<IEnumerable<IAssemblyData>> OnJobsAdded;
		private Action<FinishResult> OnJobCompleted;
		private Action<int> OnGetNextBatch;
		private Action<IEnumerable<Guid>, string, Guid> OnBatchStarted;

		public FeedProviderMock(ConcurrentDictionary<IAssemblyData, FinishResult> jobs,
			Action<IEnumerable<IAssemblyData>> onJobsAdded = null,
			Action<FinishResult> onJobCompleted = null,
			Action<int> onGetNextBatch = null,
			Action<IEnumerable<Guid>, string, Guid> onBatchStarted = null)
		{
			this.Jobs = jobs;
			this.OnJobsAdded = onJobsAdded;
			this.OnJobCompleted = onJobCompleted;
			this.OnGetNextBatch = onGetNextBatch;
			this.OnBatchStarted = onBatchStarted;
		}
		public ConcurrentDictionary<IAssemblyData, FinishResult> Jobs { get; set; }
		public int AddJobs(IEnumerable<IAssemblyData> jobs)
		{
			jobs.Aggregate(this.Jobs, (dic, j) =>
			{

				dic.TryAdd(j, new FinishResult() { Id = j.Id, Status = FinishStatus.New });
				return dic;
			});

			if (this.OnJobsAdded != null) this.OnJobsAdded(jobs);
			return this.Jobs.Count;
		}

		public int CompleteJob(FinishResult result)
		{
			var job = this.Jobs.Keys.First(it => it.Id == result.Id);
			this.Jobs[job] = result;

			if (this.OnJobCompleted != null) this.OnJobCompleted(result);
			return 1;
		}

		public IEnumerable<IAssemblyData> GetNextBatch(int size)
		{
			if (this.OnGetNextBatch != null) this.OnGetNextBatch(size);
			return this.Jobs.Keys.Take(size);
		}

		public int StartBatch(IEnumerable<Guid> ids, string machineName, Guid instanceId)
		{
			if (this.OnBatchStarted != null) this.OnBatchStarted(ids, machineName, instanceId);
			return 1;
		}
	}
}
