﻿using System;
using System.Threading.Tasks;
using Twice.Utilities;

namespace Twice.Tests
{
	internal class SyncDispatcher : IDispatcher
	{
		public void CheckBeginInvokeOnUI( Action action )
		{
			action();
		}

		public Task RunAsync( Action action )
		{
			action();
			return Task.CompletedTask;
		}
	}
}