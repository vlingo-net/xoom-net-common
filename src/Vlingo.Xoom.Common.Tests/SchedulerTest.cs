// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
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
        private readonly IScheduled<CounterHolder> scheduled;
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
            using var holder = new CounterHolder(1);
            scheduler.ScheduleOnce(scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

            holder.Completes();

            Assert.Equal(1, holder.Counter);
        }

        [Fact]
        public void TestScheduleManyHappyDelivery()
        {
            using var holder = new CounterHolder(500);
            scheduler.Schedule(scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

            holder.Completes();

            Assert.Equal(500, holder.Counter);
        }


        private class Scheduled : IScheduled<CounterHolder>
        {
            public void IntervalSignal(IScheduled<CounterHolder> scheduled, CounterHolder data)
            {
                data.Increment();
            }
        }

        private class CounterHolder : IDisposable
        {
            private readonly CountdownEvent until;

            public CounterHolder(int totalExpected)
            {
                until = new CountdownEvent(totalExpected);
            }

            public int Counter { get; private set; } = 0;

            public void Increment()
            {
                if (until.CurrentCount > 0)
                {
                    ++Counter;
                    until.Signal();   
                }
            }

            public void Completes()
            {
                try
                {
                    until.Wait();
                }
                catch { }
            }

            public void Dispose() => until.Dispose();
        }
    }
}
