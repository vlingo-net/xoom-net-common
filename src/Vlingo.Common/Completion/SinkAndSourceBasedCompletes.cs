// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Completion.Sinks;
using Vlingo.Common.Completion.Sources;

namespace Vlingo.Common.Completion
{
    public class SinkAndSourceBasedCompletes<T, TSource> : ICompletes<T>
    {
        private static long _defaultTimeout = long.MaxValue;

        private Scheduler scheduler;
        private InMemorySource<TSource> source;
        private ISource<T> currentOperation;
        private InMemorySink<TSource> sink;

        private SinkAndSourceBasedCompletes(Scheduler scheduler, InMemorySource<TSource> source, ISource<T> currentOperation, InMemorySink<TSource> sink)
        {
            this.scheduler = scheduler;
            this.source = source;
            this.currentOperation = currentOperation;
            this.sink = sink;
        }
        
        public static SinkAndSourceBasedCompletes<T, TSource> WithScheduler(Scheduler scheduler)
        {
            var source = new InMemorySource<TSource>();
            var current = new InMemorySource<T>();
            var sink = new InMemorySink<TSource>();

            source.Subscribe(sink);

            return new SinkAndSourceBasedCompletes<T, TSource>(scheduler, source, current, sink);
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

        public ICompletes<T> Ready()
        {
            throw new NotImplementedException();
        }
    }
}