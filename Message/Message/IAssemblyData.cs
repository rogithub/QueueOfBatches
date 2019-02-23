using System;
using System.Reflection;

namespace Message
{
	//https://stackoverflow.com/questions/14479074/c-sharp-reflection-load-assembly-and-invoke-a-method-if-it-exists
	public interface IAssemblyData
	{
		Guid MessageId { get; set; }
		object[] ConstructorParameters { get; set; }
		object[] MethodParameters { get; set; }
		Type[] MethodParametersTypes { get; set; }
		Assembly Assembly { get; set; }
		string MethodToRun { get; set; }
		string FullyQualifiedName { get; set; }
	}
}
