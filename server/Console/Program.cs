
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

			System.Console.WriteLine("Listenning for requests...");

			var c = new CancellationTokenSource();
			var t = c.Token;
			var service = new Agent.Service(t);
			service.Start();

			System.Console.ReadKey();
			c.Cancel();

			System.Console.ReadLine();
		}
	}
}
