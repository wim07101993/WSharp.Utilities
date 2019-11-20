using System.Linq;

namespace WSharp.Utilities.Collections
{
    public class FifoActionQueue : ActionQueue
    {
         public override bool MoveNext()
        {
            lock (Lock)
            {
                // if there ar no actions left in the current working buffer, switch
                if (!BufferToExecute.Any())
                    WorkingOnBuffer1 = !WorkingOnBuffer1;
                // if there ar also no actions left in the other buffer, return false;
                if (!BufferToExecute.Any())
                    return false;

                // if the execution of the queue has started, remove the current action
                // else set the flag to true
                if (IsExecuting)
                    BufferToExecute.RemoveAt(0);
                else
                    IsExecuting = true;

                // check again whether there are elements left in the buffers
                if (!BufferToExecute.Any())
                    WorkingOnBuffer1 = !WorkingOnBuffer1;
                if (!BufferToExecute.Any())
                    return false;

                // select the next action to execute
                Current = BufferToExecute[0];
                // execute the action
                Current(CancellationToken);

                return true;
            }
        }
    }
}
