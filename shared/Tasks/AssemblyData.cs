using System;
using System.Reflection;

namespace Tasks
{
	[Serializable]
	public class AssemblyData : IAssemblyData
	{
		public Guid Id { get; set; }
		public object[] ConstructorParameters { get; set; }
		public object[] MethodParameters { get; set; }
		public Type[] MethodParametersTypes { get; set; }
		public Assembly Assembly { get; set; }
		public string MethodToRun { get; set; }
		public string FullyQualifiedName { get; set; }
		public int TimeoutMilliseconds { get; set; }
	}
}
