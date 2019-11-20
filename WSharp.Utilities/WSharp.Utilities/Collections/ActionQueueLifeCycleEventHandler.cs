using System;
using System.Threading;

namespace WSharp.Utilities.Collections
{
    public delegate void ActionQueueLifeCycleEventHandler(ActionQueue sender, Action<CancellationToken> action);
}
