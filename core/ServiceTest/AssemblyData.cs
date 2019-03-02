using System;
using System.Reflection;
using System.Threading;
using TasksInterfaces;

namespace ServiceTest
{
	public class AssemblyTask : ITask<IAssemblyData, FinishResult>
	{
		public FinishResult Run(IAssemblyData msg, CancellationTokenSource source)
		{
			var t = msg.Assembly.GetType(msg.FullyQualifiedName);
			var o = Activator.CreateInstance(t, msg.ConstructorParameters);
			var methodInfo = t.GetMethod(msg.MethodToRun, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, msg.MethodParametersTypes, null);
			var r = methodInfo.Invoke(o, msg.MethodParameters);
			return new FinishResult(msg.Id, FinishStatus.Succes, r, null);
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
	public enum FinishStatus
	{
		New = 0,
		Succes,
		Error,
		Canceled
	}
	public struct FinishResult
	{
		public FinishResult(Guid id, FinishStatus status, object result, Exception ex)
		{
			this.Id = id;
			this.Status = status;
			this.Result = result;
			this.Exception = ex;
		}
		public Guid Id { get; set; }
		public FinishStatus Status { get; set; }
		public object Result { get; set; }
		public Exception Exception { get; set; }
	}
	public interface IAssemblyData : ITaskInput
	{
		object[] ConstructorParameters { get; set; }
		object[] MethodParameters { get; set; }
		Type[] MethodParametersTypes { get; set; }
		Assembly Assembly { get; set; }
		string MethodToRun { get; set; }
		string FullyQualifiedName { get; set; }
	}
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
