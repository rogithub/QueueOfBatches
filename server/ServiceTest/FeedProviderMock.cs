using Message;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceTest
{
	public class FeedProviderMock : IFeedProvider<IAssemblyData, FinishResult>
	{
		private Action<IAssemblyData[]> OnJobsAdded;
		private Action<FinishResult> OnJobCompleted;
		private Action<int> OnGetNextBatch;
		private Action<IEnumerable<Guid>, string, Guid> OnBatchStarted;

		public FeedProviderMock(Dictionary<IAssemblyData, FinishResult> jobs, Action<IAssemblyData[]> onJobsAdded, Action<FinishResult> onJobCompleted, Action<int> onGetNextBatch, Action<IEnumerable<Guid>, string, Guid> onBatchStarted)
		{
			this.Jobs = jobs;
			this.OnJobsAdded = onJobsAdded;
			this.OnJobCompleted = onJobCompleted;
			this.OnGetNextBatch = onGetNextBatch;
			this.OnBatchStarted = onBatchStarted;
		}
		public Dictionary<IAssemblyData, FinishResult> Jobs { get; set; }
		public int AddJobs(IAssemblyData[] jobs)
		{
			jobs.Aggregate(this.Jobs, (dic, j) =>
			{
				dic.Add(j, new FinishResult() { Id = j.Id, Status = FinishStatus.New });
				return dic;
			});

			this.OnJobsAdded(jobs);
			return this.Jobs.Count;
		}

		public int CompleteJob(FinishResult result)
		{
			var job = this.Jobs.Keys.First(it => it.Id == result.Id);
			this.Jobs[job] = result;

			this.OnJobCompleted(result);
			return 1;
		}

		public IEnumerable<IAssemblyData> GetNextBatch(int size)
		{
			this.OnGetNextBatch(size);
			return this.Jobs.Keys.Take(size);
		}

		public int StartBatch(IEnumerable<Guid> ids, string machineName, Guid instanceId)
		{
			this.OnBatchStarted(ids, machineName, instanceId);
			return 1;
		}
	}
}
