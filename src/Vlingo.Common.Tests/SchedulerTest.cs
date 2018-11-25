// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Test;
using Xunit;

namespace Vlingo.Common.Tests
{
    public class SchedulerTest : IDisposable
    {
        private readonly IScheduled scheduled;
        private readonly Scheduler scheduler;

        public SchedulerTest()
        {
            scheduled = new Scheduled();
            scheduler = new Scheduler();
        }

        public void Dispose()
        {
            scheduler.Close();
        }

        [Fact]
        public void TestScheduleOnceOneHappyDelivery()
        {
            var holder = new CounterHolder();
            holder.Until = TestUntil.Happenings(1);

            scheduler.ScheduleOnce(scheduled, holder, 0, 1);
            holder.Until.Completes();

            Assert.Equal(1, holder.Counter);
        }

        [Fact]
        public void TestScheduleManyHappyDelivery()
        {
            var holder = new CounterHolder();
            holder.Until = TestUntil.Happenings(505);

            scheduler.Schedule(scheduled, holder, 0, 1);
            holder.Until.Completes();

            Assert.True(holder.Counter > 500);
        }


        private class Scheduled : IScheduled
        {
            public void IntervalSignal(IScheduled scheduled, object data)
            {
                ((CounterHolder)data).Increment();
            }
        }

        private class CounterHolder
        {
            public int Counter { get; private set; } = 0;
            public TestUntil Until { get; set; }

            public void Increment()
            {
                ++Counter;
                Until.Happened();
            }
        }
    }
}
