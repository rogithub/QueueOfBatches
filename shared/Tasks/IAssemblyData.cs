using System;
using System.Reflection;
using TasksInterfaces;

namespace Tasks
{
	//https://stackoverflow.com/questions/14479074/c-sharp-reflection-load-assembly-and-invoke-a-method-if-it-exists
	public interface IAssemblyData : ITaskInput
	{
		object[] ConstructorParameters { get; set; }
		object[] MethodParameters { get; set; }
		Type[] MethodParametersTypes { get; set; }
		Assembly Assembly { get; set; }
		string MethodToRun { get; set; }
		string FullyQualifiedName { get; set; }
	}
}
