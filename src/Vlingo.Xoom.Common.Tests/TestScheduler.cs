// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace Vlingo.Xoom.Common.Tests
{
    public class TestScheduler : Scheduler
    {
        public override ICancellable ScheduleOnce<T>(IScheduled<T> scheduled, T data, TimeSpan delayBefore, TimeSpan interval)
        {
            var t = new Thread(() =>
            {
                Thread.Sleep(delayBefore + interval);
                scheduled.IntervalSignal(scheduled, data);
            });
            t.Start();

            return new NoOpCancellable();
        }
    }
    
    public class NoOpCancellable : ICancellable
    {
        public bool Cancel() => true;
    }
}