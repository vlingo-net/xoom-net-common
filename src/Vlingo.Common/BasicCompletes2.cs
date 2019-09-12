// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public class BasicCompletes2<T> : ICompletes<T>
    {
        private readonly T _outcome;
        private readonly bool _succeeded;
        private readonly Scheduler _scheduler;

        public BasicCompletes2(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }
        
        public BasicCompletes2(T outcome, bool succeeded)
        {
            outcome = outcome;
            succeeded = succeeded;
        }
        
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
        
        public T Outcome { get; }
        
        public ICompletes<T> Repeat()
        {
            throw new NotImplementedException();
        }
    }

    public static class Completes2
    {
        public static ICompletes<T> Using<T>(Scheduler scheduler)
        {
            return new BasicCompletes2<T>(scheduler);
        }

        public static ICompletes<T> WithSuccess<T>(T outcome)
        {
            return new BasicCompletes2<T>(outcome, true);
        }

        public static ICompletes<T> WithFailure<T>(T outcome)
        {
            return new BasicCompletes2<T>(outcome, false);
        }

        public static ICompletes<T> WithFailure<T>()
        {
            return new BasicCompletes2<T>(default(T), false);
        }
    }
}