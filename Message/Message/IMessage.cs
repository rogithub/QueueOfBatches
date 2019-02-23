using System;

namespace Message
{
	public interface IMessage
	{
		void OnExecute(int payload);
		int Param { get; }
	}
}
