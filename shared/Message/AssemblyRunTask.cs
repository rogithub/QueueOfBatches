using System;
using System.Reflection;

namespace Message
{
	public class AssemblyRunTask : IMessageTask<IAssemblyData, FinishResult>
	{
		public FinishResult OnRun(IAssemblyData msg)
		{
			try
			{
				var t = msg.Assembly.GetType(msg.FullyQualifiedName);
				var methodInfo = t.GetMethod(msg.MethodToRun, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, msg.MethodParametersTypes, null);
				var o = Activator.CreateInstance(t, msg.ConstructorParameters);
				var r = methodInfo.Invoke(o, msg.MethodParameters);
				return new FinishResult(msg.Id, FinishStatus.Succes, r, null);
			}
			catch (Exception ex)
			{
				return OnError(msg, ex);
			}
		}

		public FinishResult OnError(IAssemblyData msg, Exception ex)
		{
			return new FinishResult(msg.Id, FinishStatus.Error, null, ex);
		}

		public FinishResult OnCancell(IAssemblyData msg, Exception ex)
		{
			return new FinishResult(msg.Id, FinishStatus.Canceled, null, ex);
		}
	}
}
