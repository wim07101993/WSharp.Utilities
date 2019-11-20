using System;
using System.Collections;
using System.Collections.Generic;

namespace WSharp.Utilities.Collections
{
    public class ActionSequence<TKey> : IDictionary<TKey, ActionSequenceAction<TKey>>, IEnumerable<ActionSequenceAction<TKey>>
        where TKey : IEquatable<TKey>
    {
        #region FIELDS

        protected readonly object _lock = new object();
        private readonly IDictionary<TKey, ActionSequenceAction<TKey>> _actions = new Dictionary<TKey, ActionSequenceAction<TKey>>();

        protected int _version;

        #endregion FIELDS

        #region PROPERTIES

        public TKey Start { get; set; }
        public TKey End { get; set; }

        #endregion PROPERTIES

        #region METHODS

        public void Execute()
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext()) { }
        }

        public virtual ActionSequence<TKey> BeforeStart(ActionSequenceLifeCycleEventHandler<TKey> action)
        {
            lock (_lock)
            {
                SequenceStarted += action;
                return this;
            }
        }

        public virtual ActionSequence<TKey> BeforeStep(ActionSequenceLifeCycleEventHandler<TKey> action)
        {
            lock (_lock)
            {
                ExecutingAction += action;
                return this;
            }
        }

        public virtual ActionSequence<TKey> AfterStep(ActionSequenceLifeCycleEventHandler<TKey> action)
        {
            lock (_lock)
            {
                ExecutedAction += action;
                return this;
            }
        }

        public virtual ActionSequence<TKey> AfterEnd(ActionSequenceLifeCycleEventHandler<TKey> action)
        {
            lock (_lock)
            {
                SequenceEnded += action;
                return this;
            }
        }

        protected virtual void OnSequenceStarted() => SequenceStarted?.Invoke(this, Start, this[Start]);

        protected virtual void OnExecutingAction(TKey key, ActionSequenceAction<TKey> action) => ExecutingAction?.Invoke(this, key, action);

        protected virtual void OnExecutedAction(TKey key, ActionSequenceAction<TKey> action) => ExecutedAction?.Invoke(this, key, action);

        protected virtual void OnSequenceEnded() => SequenceEnded?.Invoke(this, End, this[End]);

        #endregion METHODS

        #region EVENTS

        public virtual event ActionSequenceLifeCycleEventHandler<TKey> SequenceStarted;

        public virtual event ActionSequenceLifeCycleEventHandler<TKey> ExecutingAction;

        public virtual event ActionSequenceLifeCycleEventHandler<TKey> ExecutedAction;

        public virtual event ActionSequenceLifeCycleEventHandler<TKey> SequenceEnded;

        #endregion EVENTS

        #region ENUMERABLE

        public virtual IEnumerator<ActionSequenceAction<TKey>> GetEnumerator() => new ActionSequenceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion ENUMERABLE

        #region IDICTIONARY

        public virtual ICollection<TKey> Keys => _actions.Keys;
        public virtual ICollection<ActionSequenceAction<TKey>> Values => _actions.Values;
        public virtual int Count => _actions.Count;
        public virtual bool IsReadOnly => false;

        public virtual ActionSequenceAction<TKey> this[TKey key]
        {
            get => _actions[key];
            set
            {
                if (ContainsKey(key))
                {
                    _actions[key] = value;
                    _version++;
                }
                else
                    Add(key, value);
            }
        }

        public virtual bool ContainsKey(TKey key) => _actions.ContainsKey(key);

        public virtual bool Contains(KeyValuePair<TKey, ActionSequenceAction<TKey>> item) => _actions.Contains(item);

        public virtual bool TryGetValue(TKey key, out ActionSequenceAction<TKey> value) => _actions.TryGetValue(key, out value);

        public virtual void Add(TKey key, ActionSequenceAction<TKey> value)
        {
            lock (_lock)
            {
                _actions.Add(key, value);
                _version++;
            }
        }

        public virtual void Add(KeyValuePair<TKey, ActionSequenceAction<TKey>> item)
        {
            lock (_lock)
            {
                _actions.Add(item);
                _version++;
            }
        }

        public virtual bool Remove(TKey key)
        {
            lock (_lock)
            {
                var removed = _actions.Remove(key);
                _version++;
                return removed;
            }
        }

        public virtual bool Remove(KeyValuePair<TKey, ActionSequenceAction<TKey>> item)
        {
            lock (_lock)
            {
                var removed = _actions.Remove(item);
                _version++;
                return removed;
            }
        }

        public virtual void Clear()
        {
            lock (_lock)
            {
                _actions.Clear();
                _version++;
            }
        }

        public virtual void CopyTo(KeyValuePair<TKey, ActionSequenceAction<TKey>>[] array, int arrayIndex) => _actions.CopyTo(array, arrayIndex);

        IEnumerator<KeyValuePair<TKey, ActionSequenceAction<TKey>>> IEnumerable<KeyValuePair<TKey, ActionSequenceAction<TKey>>>.GetEnumerator() => _actions.GetEnumerator();

        #endregion IDICTIONARY

        #region CLASSES

        private struct ActionSequenceEnumerator : IEnumerator<ActionSequenceAction<TKey>>, IEnumerator
        {
            private readonly ActionSequence<TKey> _sequence;
            private readonly int _version;

            private TKey _key;
            private bool _started;

            public ActionSequenceEnumerator(ActionSequence<TKey> sequence)
            {
                _sequence = sequence;
                _version = sequence._version;

                _started = false;

                _key = sequence.Start;
                Current = sequence[sequence.Start];
            }

            public ActionSequenceAction<TKey> Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                lock (_sequence._lock)
                {
                    if (_sequence._version != _version)
                        throw new InvalidOperationException("The sequence changed while iterating over it...");

                    if (_key.Equals(_sequence.Start))
                        _sequence.OnSequenceStarted();

                    if (_started)
                        Current = _sequence[_key];
                    else
                    {
                        _started = true;
                    }

                    _sequence.OnExecutingAction(_key, Current);
                    Current.Execute();
                    _sequence.OnExecutedAction(_key, Current);

                    if (_key.Equals(_sequence.End))
                    {
                        _sequence.OnSequenceEnded();
                        return false;
                    }

                    _key = Current.NextAction;
                    return true;
                }
            }

            public void Reset()
            {
                _key = _sequence.Start;
                Current = _sequence[_key];
                _started = false;
            }
        }

        #endregion CLASSES
    }
}