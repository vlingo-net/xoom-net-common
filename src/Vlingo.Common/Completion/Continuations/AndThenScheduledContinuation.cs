// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Continuations
{
    internal sealed class AndThenScheduledContinuation<TAntecedentResult, TResult> : AndThenContinuation<TAntecedentResult, TResult>, IScheduled<object>
    {
        private readonly Scheduler scheduler;
        private readonly TimeSpan timeout;
        private ICancellable cancellable;
        private readonly AtomicBoolean executed = new AtomicBoolean(false);
        private readonly AtomicBoolean timedOut = new AtomicBoolean(false);

        internal AndThenScheduledContinuation(
            Scheduler scheduler,
            BasicCompletes parent,
            BasicCompletes<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Delegate function)
            : base(parent, antecedent, function)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
            ClearTimer();
            StartTimer();
        }
        
        internal AndThenScheduledContinuation(
            Scheduler scheduler,
            BasicCompletes parent,
            BasicCompletes<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Optional<TResult> failedOutcomeValue,
            Delegate function)
            : base(parent, antecedent, failedOutcomeValue, function)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
            ClearTimer();
            StartTimer();
        }

        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (timedOut.Get())
            {
                return;
            }
            
            base.InnerInvoke(completedCompletes);
            executed.Set(true);
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            if (!executed.Get())
            {
                timedOut.Set(true);
                HasFailedValue.Set(true);
            }
        }

        private void StartTimer()
        {
            if (timeout.TotalMilliseconds > 0 && scheduler != null)
            {
                // 2ms delayBefore prevents timeout until after return from here
                cancellable = scheduler.ScheduleOnce(this, null, TimeSpan.FromMilliseconds(2), timeout);
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