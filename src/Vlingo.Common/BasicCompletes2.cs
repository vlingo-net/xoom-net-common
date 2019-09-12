using System;

namespace Vlingo.Common
{
    public class BasicCompletes2<T> : ICompletes<T>
    {
        private readonly T _outcome;

        public BasicCompletes2(T outcome)
        {
            _outcome = outcome;
        }

        public ICompletes<TO> With<TO>(TO outcome)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(TimeSpan timeout, TO failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(TO failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(TimeSpan timeout, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(TimeSpan timeout, T failedOutcomeValue, Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(T failedOutcomeValue, Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(TimeSpan timeout, Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TF, TO>(TimeSpan timeout, TF failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TF, TO>(TF failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TO>(TimeSpan timeout, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TO>(Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> Otherwise(Func<T, T> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> OtherwiseConsume(Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> RecoverFrom(Func<Exception, T> function)
        {
            throw new NotImplementedException();
        }

        public TO Await<TO>()
        {
            throw new NotImplementedException();
        }

        public TO Await<TO>(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool IsCompleted { get; }
        
        public bool HasFailed { get; }
        
        public void Failed()
        {
            throw new NotImplementedException();
        }

        public bool HasOutcome { get; }
        
        public T Outcome => _outcome;
        
        public ICompletes<T> Repeat()
        {
            throw new NotImplementedException();
        }
    }
}