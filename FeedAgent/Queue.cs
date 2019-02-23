using System;
using System.Collections.Generic;
using System.Text;
using JobProcessor;
using Message;
using Microsoft.FSharp.Control;

namespace FeedAgent
{
	public class Queue
	{
		public Queue()
		{

		}
		public void Post(IMessage msg)
		{
			Agent.Queue.Post(msg);
		}
	}
}
