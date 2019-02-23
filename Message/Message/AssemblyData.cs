using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Message
{
	public class AssemblyData : IAssemblyData
	{
		public Guid MessageId { get; set; }
		public object[] ConstructorParameters { get; set; }
		public object[] MethodParameters { get; set; }
		public Type[] MethodParametersTypes { get; set; }
		public Assembly Assembly { get; set; }
		public string MethodToRun { get; set; }
		public string FullyQualifiedName { get; set; }
	}
}
