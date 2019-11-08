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
            
            Parent.DiagnosticCollector.Append($"InnerInvoke will execute");
            base.InnerInvoke(completedCompletes);
            executed.Set(true);
        }

        public void IntervalSignal(IScheduled<object?> scheduled, object? data)
        {
            if (executed.Get() && !TimedOut.Get())
            {
                Parent.DiagnosticCollector.StopAppendStart($"Want to time out but already executed '{((TimeSpan)data!).TotalMilliseconds}ms'");
            }
            
            if (!executed.Get() && !TimedOut.Get())
            {
                Parent.DiagnosticCollector.StopAppendStart($"IntervalSignal timouting with expected '{((TimeSpan)data!).TotalMilliseconds}ms'");
                TimedOut.Set(true);
                Parent.TimedOut.Set(true);
                HasFailedValue.Set(true);
            }
        }

        private void StartTimer()
        {
            if (timeout.TotalMilliseconds > 0 && Parent.Scheduler != null)
            {
                Parent.DiagnosticCollector.StartAppend($"StartTimer with expected timeout of '{timeout.TotalMilliseconds}ms'");
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