using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest
{
	public class IntAssemblyMethodMock
	{
		Func<int, int, int> MethodToRun { get; set; }
		public IntAssemblyMethodMock(Func<int, int, int> methodToRun)
		{
			this.MethodToRun = methodToRun;
		}

		public int Method(int a, int b)
		{
			return this.MethodToRun(a, b);
		}
	}
}
