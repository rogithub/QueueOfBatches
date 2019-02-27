
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
			IFeedProvider provider = new DbFeedProvider();
			var c = new CancellationTokenSource();
			Agent.InitData data = new Agent.InitData(c.Token, provider, AppSettings.MillisecondsToBeIdle, AppSettings.BatchSize, Guid.NewGuid(), System.Environment.MachineName);
			var service = new Agent.Service(data);

			service.Start();
			System.Console.WriteLine("{0} instance {1} Listenning...", data.InstanceName, data.InstanceId);

			System.Console.ReadKey();
			c.Cancel();

			System.Console.ReadKey();
		}
	}
}
