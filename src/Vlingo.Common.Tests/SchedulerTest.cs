// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Xunit;

namespace Vlingo.Common.Tests
{
    public class SchedulerTest : IDisposable
    {
        private readonly IScheduled scheduled;
        private readonly Scheduler scheduler;
        private CountdownEvent countDown;

        public SchedulerTest()
        {
            scheduled = new Scheduled();
            scheduler = new Scheduler();
            countDown = new CountdownEvent(1);
        }

        public void Dispose()
        {
            scheduler.Close();
            countDown.Dispose();
        }

        [Fact]
        public void TestScheduleOnceOneHappyDelivery()
        {
            var holder = new CounterHolder();
            countDown.Reset(1);
            holder.Until = countDown;

            scheduler.ScheduleOnce(scheduled, holder, 0, 1);
            holder.Until.Wait();

            Assert.Equal(1, holder.Counter);
        }

        [Fact]
        public void TestScheduleManyHappyDelivery()
        {
            var holder = new CounterHolder();
            countDown.Reset(505);
            holder.Until = countDown;

            scheduler.Schedule(scheduled, holder, 0, 1);
            holder.Until.Wait();

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
            public CountdownEvent Until { get; set; }

            public void Increment()
            {
                ++Counter;
                Until.Signal();
            }
        }
    }
}
