// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completes.Barrier
{
    public sealed class TimeBarrier
    {
        private readonly Scheduler scheduler;
        private readonly TimeSpan timeout;
        private AtomicBoolean didTimeout;
        private ICancellable timeoutCancellable;

        public TimeBarrier(Scheduler scheduler, TimeSpan timeout)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
            didTimeout = new AtomicBoolean(false);
        }

        public void Initialize()
        {
            timeoutCancellable = scheduler?.ScheduleOnce(new TimeBarrierScheduledTask(), didTimeout, TimeSpan.Zero, timeout);
        }

        public void Execute(IRunnable section, IOperation nextOperation)
        {
            if(scheduler == null)
            {
                section.Run();
            }
            else
            {
                if (!didTimeout.Get())
                {
                    section.Run();
                    timeoutCancellable.Cancel();
                }
                else
                {
                    nextOperation.OnError(new TimeoutException());
                }

                didTimeout.Set(false);
            }
        }

        private class TimeBarrierScheduledTask : IScheduled
        {
            public void IntervalSignal(IScheduled scheduled, object data) => (data as AtomicBoolean).Set(true);
        }
    }
}
