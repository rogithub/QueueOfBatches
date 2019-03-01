using Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceTest
{
	public static class TaskFactory
	{
		public static IEnumerable<IAssemblyData> Create(IEnumerable<Func<int, int, int>> methods, IEnumerable<int[]> parameters, int timeoutms)
		{
			for (int i = 0; i < methods.Count(); i++)
			{
				Guid id = Guid.NewGuid();
				yield return new AssemblyData()
				{

					TimeoutMilliseconds = timeoutms,
					Assembly = Assembly.GetAssembly(typeof(IntAssemblyMethodMock)),
					ConstructorParameters = new object[] { methods.ElementAt(i) },
					FullyQualifiedName = "ServiceTest.IntAssemblyMethodMock",
					Id = Guid.NewGuid(),
					MethodParameters = new object[] { parameters.ElementAt(i)[0], parameters.ElementAt(i)[1] },
					MethodParametersTypes = new Type[] { typeof(int), typeof(int) },
					MethodToRun = "Method" //two generic params --> `2
				};
			}
		}
	}
}
