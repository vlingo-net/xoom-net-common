// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Continuations
{
    internal sealed class AndThenScheduledContinuation<TAntecedentResult, TResult> : AndThenContinuation<TAntecedentResult, TResult>, IScheduled<object?>
    {
        private readonly TimeSpan timeout;
        private ICancellable? cancellable;
        private readonly AtomicBoolean executed = new AtomicBoolean(false);
        private volatile object innerLock = new object();
        private volatile object internvalLock = new object();

        internal AndThenScheduledContinuation(
            BasicCompletes parent,
            BasicCompletes<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Delegate function)
            : this(parent, antecedent, timeout, Optional.Empty<TResult>(), function)
        {
        }
        
        internal AndThenScheduledContinuation(
            BasicCompletes? parent,
            BasicCompletes<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Optional<TResult> failedOutcomeValue,
            Delegate function)
            : base(parent, antecedent, failedOutcomeValue, function)
        {
            this.timeout = timeout;
            ClearTimer();
            StartTimer();
        }

        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            lock (innerLock)
            {
                if (TimedOut.Get() || executed.Get())
                {
                    // Console.WriteLine($"Returning from execution: TimedOut {TimedOut.Get()}, executed {executed.Get()} | thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                    return;
                }
            
                base.InnerInvoke(completedCompletes);
                executed.Set(true);
                // Console.WriteLine($"Executed | thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");   
            }
        }

        public void IntervalSignal(IScheduled<object?> scheduled, object? data)
        {
            lock (internvalLock)
            {
                if (!executed.Get() && !TimedOut.Get())
                {
                    // Console.WriteLine($"Timeout set | thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                    TimedOut.Set(true);
                    Parent.TimedOut.Set(true);
                    HasFailedValue.Set(true);
                }   
            }
        }

        private void StartTimer()
        {
            // Console.WriteLine($"Timeout started | thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            if (timeout.TotalMilliseconds > 0 && Parent.Scheduler != null)
            {
                cancellable = Parent.Scheduler.ScheduleOnce(this, null, TimeSpan.FromMilliseconds(5), timeout);
            }
        }

        private void ClearTimer()
        {
            if (cancellable != null)
            {
                cancellable.Cancel();
                cancellable = null;
            }
        }
    }
}