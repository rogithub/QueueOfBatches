﻿using DataBase;
using Examples;
using Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Console
{
	class Program
	{
		public static IEnumerable<IAssemblyData> CreateFile(int count)
		{
			for (int i = 0; i < count; i++)
			{
				Guid id = Guid.NewGuid();
				yield return new AssemblyData()
				{
					TimeoutMilliseconds = -1, //-1 infinite
					Assembly = Assembly.GetAssembly(typeof(FileCreator)),
					ConstructorParameters = new object[] { @"C:\Users\43918\Desktop\test" },
					FullyQualifiedName = "Examples.FileCreator",
					Id = id,
					MethodParameters = new object[] { id },
					MethodParametersTypes = new Type[] { typeof(Guid) },
					MethodToRun = "CreateFile"
				};
			}
		}

		static void Main(string[] args)
		{
			DbQueue queue = new DbQueue();
			bool continueRunning = true;
			while (continueRunning)
			{
				System.Console.WriteLine("Items to create (if not parseable int will exit)");
				string value = System.Console.ReadLine();
				int itemsToCreate = 0;
				continueRunning = int.TryParse(value, out itemsToCreate);
				if (continueRunning)
				{
					queue.Enqueue(CreateFile(itemsToCreate).ToArray());
					System.Console.WriteLine("Created: {0}", itemsToCreate);
				}
			}
		}
	}
}
