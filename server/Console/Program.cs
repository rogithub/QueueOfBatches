
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

			var c = new CancellationTokenSource();
			var t = c.Token;
			IFeedProvider provider = new DbFeedProvider();
			var service = new Agent.Service(t, provider, AppSettings.MillisecondsToBeIdle, AppSettings.BatchSize, Guid.NewGuid());


			service.Start();
			System.Console.WriteLine("{0} instance {1} Listenning...", service.MachineName, service.InstanceId);

			System.Console.ReadKey();
			c.Cancel();

			System.Console.ReadKey();
		}
	}
}
