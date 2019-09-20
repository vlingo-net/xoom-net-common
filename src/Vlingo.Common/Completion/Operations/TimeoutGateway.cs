// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Operations
{
    public class TimeoutGateway<T> : Operation<T, T>, IScheduled<object>
    {
        private Scheduler scheduler;
        private TimeSpan timeout;
        private ICancellable cancellable;
        private AtomicBoolean didTimeout;

        public TimeoutGateway(Scheduler scheduler, TimeSpan timeout)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
            this.didTimeout = new AtomicBoolean(false);
            StartTimer();
        }
        
        public override void OnOutcome(T receives)
        {
            if (!didTimeout.Get())
            {
                this.cancellable.Cancel();
                EmitOutcome(receives);
                StartTimer();
            }
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            EmitError(new TimeoutException());
            didTimeout.Set(true);
        }
        
        private void StartTimer()
        {
            this.cancellable = scheduler.ScheduleOnce(this, null, TimeSpan.Zero, timeout);
            this.didTimeout.Set(false);
        }
    }
}