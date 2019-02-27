
using JobProcessor;
using Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Console
{
	class Program
	{

		static void Main(string[] args)
		{

			var c = new CancellationTokenSource();
			var t = c.Token;
			var service = new Agent.Service(t);
			service.Start();
			System.Console.WriteLine("{0} instance {0} Listenning...", service.MachineName, service.InstanceId);

			System.Console.ReadKey();
			c.Cancel();

			System.Console.ReadLine();
		}
	}
}
