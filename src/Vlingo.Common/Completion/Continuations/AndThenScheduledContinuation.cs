// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;

namespace Vlingo.Common.Completion.Continuations
{
    internal sealed class AndThenScheduledContinuation<TAntecedentResult, TResult> : AndThenContinuation<TAntecedentResult, TResult>, IScheduled<object?>
    {
        private readonly TimeSpan timeout;
        private ICancellable? cancellable;
        private readonly AtomicBoolean executed = new AtomicBoolean(false);
        private Stopwatch _stopwatch = new Stopwatch();

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
            if (TimedOut.Get() || executed.Get())
            {
                return;
            }
        
            base.InnerInvoke(completedCompletes);
            executed.Set(true);
        }

        public void IntervalSignal(IScheduled<object?> scheduled, object? data)
        {
            if (!executed.Get() && !TimedOut.Get())
            {
                _stopwatch.Stop();
                if (_stopwatch.ElapsedMilliseconds > ((TimeSpan) data).TotalMilliseconds)
                {
                    Console.WriteLine($"Scheduled timeout {((TimeSpan) data).TotalMilliseconds} but elapsed scheduled time {_stopwatch.ElapsedMilliseconds}");
                }
                TimedOut.Set(true);
                Parent.TimedOut.Set(true);
                HasFailedValue.Set(true);
            }   
        }

        private void StartTimer()
        {
            if (timeout.TotalMilliseconds > 0 && Parent.Scheduler != null)
            {
                _stopwatch.Start();
                cancellable = Parent.Scheduler.ScheduleOnce(this, timeout, TimeSpan.Zero, timeout);
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