using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Examples;
using JobProcessor;
using Message;


namespace Console
{
	class Program
	{
		public static IAssemblyData CreateFile()
		{

			IAssemblyData m = new AssemblyData();
			m.Assembly = Assembly.GetAssembly(typeof(FileCreator));
			m.ConstructorParameters = new object[] { @"C:\Users\43918\Desktop\test" };
			m.FullyQualifiedName = "Examples.FileCreator";
			m.MessageId = Guid.NewGuid();
			m.MethodParameters = new object[] { m.MessageId };
			m.MethodParametersTypes = new Type[] { typeof(Guid) };
			m.MethodToRun = "CreateFile";
			return m;
		}

		static void Main(string[] args)
		{
			for (int i = 0; i < 100; i++)
			{
				IAssemblyData data = CreateFile();
				Agent.AssemblyRunner.Post(data);
			}

			System.Console.WriteLine("Creating 100 files...");
			System.Console.ReadLine();
		}
	}
}
