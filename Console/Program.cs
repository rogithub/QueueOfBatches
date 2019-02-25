using Examples;
using JobProcessor;
using Message;
using Microsoft.FSharp.Core;
using System;
using System.Reflection;

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

			Action<FinishResult> action = (result) => System.Console.Write(result.ToString());
			System.Console.WriteLine("Creating 1 files...");
			Agent.Process(CreateFile(), FuncConvert.ToFSharpFunc(action));
			System.Console.ReadLine();
		}
	}
}
