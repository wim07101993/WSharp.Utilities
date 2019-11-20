using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace WSharp.Utilities.Collections
{
    public abstract class ActionQueue : ICollection<Action<CancellationToken>>, INotifyCollectionChanged, IEnumerator<Action<CancellationToken>>
    {
        #region FIELDS

        private readonly List<Action<CancellationToken>> _buffer1 = new List<Action<CancellationToken>>();
        private readonly List<Action<CancellationToken>> _buffer2 = new List<Action<CancellationToken>>();

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        #endregion FIELDS

        #region PROPERTIES

        protected bool WorkingOnBuffer1 { get; set; }

        public object Lock { get; } = new object();

        protected List<Action<CancellationToken>> BufferToModify
        {
            get
            {
                lock (Lock)
                    return WorkingOnBuffer1
                        ? _buffer2
                        : _buffer1;
            }
        }

        protected List<Action<CancellationToken>> BufferToExecute
        {
            get
            {
                lock (Lock)
                    return WorkingOnBuffer1
                        ? _buffer1
                        : _buffer2;
            }
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public bool IsExecuting { get; protected set; }

        public bool StopRequested { get; protected set; }

        #endregion PROPERTIES

        #region METHODS

        public virtual void CancelExecution()
        {
            _cancellationTokenSource.Cancel();
            StopRequested = true;
        }

        public virtual void Execute()
        {
            while (MoveNext() && !StopRequested && !_cancellationTokenSource.IsCancellationRequested) { }

            lock (Lock)
            {
                IsExecuting = false;
                StopRequested = false;

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        public virtual void Stop()
        {
            lock (Lock)
            {
                StopRequested = true;
            }
        }

        public Task ExecuteAsync() => Task.Factory.StartNew(Execute);

        public virtual ActionQueue BeforeStart(ActionQueueLifeCycleEventHandler action)
        {
            lock (Lock)
            {
                SequenceStarted += action;
                return this;
            }
        }

        public virtual ActionQueue BeforeStep(ActionQueueLifeCycleEventHandler action)
        {
            lock (Lock)
            {
                ExecutingAction += action;
                return this;
            }
        }

        public virtual ActionQueue AfterStep(ActionQueueLifeCycleEventHandler action)
        {
            lock (Lock)
            {
                ExecutedAction += action;
                return this;
            }
        }

        public virtual ActionQueue AfterEnd(ActionQueueLifeCycleEventHandler action)
        {
            lock (Lock)
            {
                SequenceEnded += action;
                return this;
            }
        }

        protected virtual void OnSequenceStarted(Action<CancellationToken> action) => SequenceStarted?.Invoke(this, action);

        protected virtual void OnExecutingAction(Action<CancellationToken> action) => ExecutingAction?.Invoke(this, action);

        protected virtual void OnExecutedAction(Action<CancellationToken> action) => ExecutedAction?.Invoke(this, action);

        protected virtual void OnSequenceEnded(Action<CancellationToken> action) => SequenceEnded?.Invoke(this, action);

        #endregion METHODS

        #region EVENTS

        public virtual event ActionQueueLifeCycleEventHandler SequenceStarted;

        public virtual event ActionQueueLifeCycleEventHandler ExecutingAction;

        public virtual event ActionQueueLifeCycleEventHandler ExecutedAction;

        public virtual event ActionQueueLifeCycleEventHandler SequenceEnded;

        #endregion EVENTS

        #region INotifyCollectionChanged

        protected virtual void RaiseResetCollectionChanged()
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        protected virtual void RaiseAddCollectionChanged(Action<CancellationToken> newItem)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem));

        protected virtual void RaiseRemoveCollectionChanged(Action<CancellationToken> oldItem)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem));

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion INotifyCollectionChanged

        #region ICollection<Action<CancellationToken>>

        public virtual int Count
        {
            get
            {
                lock (Lock)
                    return _buffer1.Count + _buffer2.Count;
            }
        }

        public virtual bool IsReadOnly => false;

        public virtual void Add(Action<CancellationToken> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Cannot add null to queue");

            lock (Lock)
            {
                BufferToModify.Add(item);
                RaiseAddCollectionChanged(item);
            }
        }

        public virtual bool Remove(Action<CancellationToken> item)
        {
            lock (Lock)
            {
                var removed = BufferToModify.Remove(item) || BufferToExecute.Remove(item);
                RaiseRemoveCollectionChanged(item);
                return removed;
            }
        }

        public virtual void Clear()
        {
            lock (Lock)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                }
                catch (Exception e)
                {
                    // IGNORE
                }
                BufferToModify.Clear();
                BufferToExecute.Clear();
            }
        }

        public virtual bool Contains(Action<CancellationToken> item)
        {
            lock (Lock)
                return _buffer1.Contains(item) || _buffer2.Contains(item);
        }

        public virtual void CopyTo(Action<CancellationToken>[] array, int arrayIndex)
        {
            lock (Lock)
            {
                if (array.Length - arrayIndex < Count)
                    throw new IndexOutOfRangeException("The array size is not large enough to contain this collection");

                BufferToExecute.CopyTo(array, arrayIndex);
                BufferToModify.CopyTo(array, arrayIndex + BufferToExecute.Count);
            }
        }

        public IEnumerator<Action<CancellationToken>> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        #endregion ICollection<Action<CancellationToken>>

        #region IEnumerator<Action>

        public Action<CancellationToken> Current { get; protected set; }
        object IEnumerator.Current => Current;

        public abstract bool MoveNext();

        public virtual void Reset() => throw new InvalidOperationException("Cannot reset the collection");

        public virtual void Dispose()
        {
            lock (Lock)
            {
                CancelExecution();
                Clear();
            }
        }

        #endregion IEnumerator<Action>
    }
}