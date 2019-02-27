
using DataBase;
using JobProcessor;
using Message;
using System;
using System.Threading;

namespace Console
{
	class Program
	{

		static void Main(string[] args)
		{
			IFeedProvider<IAssemblyData, FinishResult> provider = new DbFeedProvider();
			var c = new CancellationTokenSource();
			var task = new AssemblyRunTask();
			var data = new Agent.InitData<IAssemblyData, FinishResult>(task, c.Token, provider, AppSettings.MillisecondsToBeIdle, AppSettings.BatchSize, Guid.NewGuid(), Environment.MachineName);
			var service = new Agent.Service<IAssemblyData, FinishResult>(data);

			service.Start();
			System.Console.WriteLine("{0} instance {1} Listenning...", data.InstanceName, data.InstanceId);

			System.Console.ReadKey();
			c.Cancel();

			System.Console.ReadKey();
		}
	}
}
