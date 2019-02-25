using Examples;
using JobProcessor;
using Message;
using Microsoft.FSharp.Core;
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
					Assembly = Assembly.GetAssembly(typeof(FileCreator)),
					ConstructorParameters = new object[] { @"C:\Users\43918\Desktop\test" },
					FullyQualifiedName = "Examples.FileCreator",
					MessageId = id,
					MethodParameters = new object[] { id },
					MethodParametersTypes = new Type[] { typeof(Guid) },
					MethodToRun = "CreateFile"
				};
			}
		}

		static void Main(string[] args)
		{
			int count = 10;
			DataBase.DbFeedProvider.Save(CreateFile(count).ToArray());
			IEnumerable<IAssemblyData> list = DataBase.DbFeedProvider.GetNextBatch();
			Action<FinishResult> action = (result) => DataBase.DbFeedProvider.Update(result);

			foreach (IAssemblyData data in list)
			{
				// -1 means infinite
				Agent.Process(-1, data, FuncConvert.ToFSharpFunc(action));
			}



			System.Console.WriteLine("Creating {0} files...", count);

			System.Console.ReadLine();
		}
	}
}
