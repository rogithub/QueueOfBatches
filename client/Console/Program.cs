using DataBase;
using Examples;
using Message;
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
					MessageId = id,
					MethodParameters = new object[] { id },
					MethodParametersTypes = new Type[] { typeof(Guid) },
					MethodToRun = "CreateFile"
				};
			}
		}

		static void Main(string[] args)
		{
			int count = 100;
			System.Console.WriteLine("Creating {0} files...", count);
			DbFeedProvider.Save(CreateFile(count).ToArray());
			System.Console.WriteLine("Done.", count);
			System.Console.ReadLine();
		}
	}
}
