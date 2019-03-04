using DataBase;
using TaskRunner;
using Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using TasksInterfaces;
using System.Linq;

namespace Console
{
	class Program
	{

		static void Main(string[] args)
		{

			ConsoleTraceListener listener = new ConsoleTraceListener();
			ITaskQueue<IAssemblyData, FinishResult> provider = new DbQueue();
			var c = new CancellationTokenSource();
			var task = new AssemblyTask();
			var data = new Agent.InitData<IAssemblyData, FinishResult>(task, c.Token, provider, AppSettings.MillisecondsToBeIdle, AppSettings.BatchSize, Guid.NewGuid(), Environment.MachineName, listener);
			var service = new Agent.Service<IAssemblyData, FinishResult>(data);

			service.Start();
			System.Console.WriteLine("{0} instance {1} Listenning...", data.InstanceName, data.InstanceId);

			System.Console.ReadKey();
			service.Stop();

			System.Console.ReadKey();
			service.Start();

			System.Console.ReadKey();
			c.Cancel();

			System.Console.ReadKey();
		}
	}
}
