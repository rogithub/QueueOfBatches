using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
	public interface IMessageTask<input, output>
	{
		output OnRun(input input);
		output OnError(input input, Exception ex);
		output OnCancell(input input, Exception ex);
	}
}
