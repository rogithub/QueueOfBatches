using Examples;
using JobProcessor;
using Message;
using System;
using System.Reflection;

namespace Console
{
	class Program
	{
		public static AssemblyData CreateFile()
		{

			AssemblyData m = new AssemblyData();
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
			System.Console.WriteLine("Creating 1 files...");

			byte[] serialized = Serializer.Serialize(CreateFile());
			System.Console.Write(serialized);

			AssemblyData data = Serializer.Deserialize<AssemblyData>(serialized);
			Agent.AssemblyRunner.Post(data);


			System.Console.ReadLine();
		}
	}
}
