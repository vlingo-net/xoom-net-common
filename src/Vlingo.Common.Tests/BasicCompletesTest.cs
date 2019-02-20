// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
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
    public class BasicCompletesTest
    {
        [Fact]
        public void TestCompletesWith()
        {
            var completes = new BasicCompletes<int>(5);

            Assert.Equal(5, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterFunction()
        {
            var completes = new BasicCompletes<int>(0);
            completes.AndThen(value => value * 2);

            completes.With(5);

            Assert.Equal(10, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterConsumer()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(0);
            completes.AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(5, andThenValue);
        }

        [Fact]
        public void TestCompletesAfterAndThen()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(0);
            completes
                .AndThen(value => value * 2)
                .AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestCompletesAfterAndThenMessageOut()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(0);
            var sender = new Sender(x => andThenValue = x);

            completes
                .AndThen(value => value * 2)
                .AndThen(x => { sender.Send(x); return x; });

            completes.With(5);

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestOutcomeBeforeTimeout()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(1000), value => value * 2)
                .AndThen(x => andThenValue = x);

            completes.With(5);
            completes.Await(TimeSpan.FromMilliseconds(10));

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestTimeoutBeforeOutcome()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(1), -10, value => value * 2)
                .AndThen(x => andThenValue = x);

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();

            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.NotEqual(10, andThenValue);
            Assert.Equal(0, andThenValue);
        }

        [Fact]
        public void TestThatFailureOutcomeFails()
        {
            int andThenValue = -1, failureValue = 0;
            var completes = new BasicCompletes<int>(new Scheduler());
            completes
                .AndThen(-100, value => 2 * value)
                .AndThen(x => andThenValue = x)
                .Otherwise(x => failureValue = 1000);

            completes.With(-100);
            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(-1, andThenValue);
            Assert.Equal(1000, failureValue);
        }

        [Fact]
        public void TestThatExceptionOutcomeFails()
        {
            int failureValue = -1;
            var completes = new BasicCompletes<int>(new Scheduler());

            completes
                .AndThen(42, value => value * 2)
                .AndThen<int>(value => throw new ApplicationException((2 * value).ToString()))
                .RecoverFrom(e => failureValue = int.Parse(e.Message));

            completes.With(2);
            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(8, failureValue);
        }

        [Fact]
        public void TestThatAwaitTimesOut()
        {
            var completes = new BasicCompletes<int>(new Scheduler());

            var completed = completes.Await(TimeSpan.FromMilliseconds(10));

            completes.With(5);

            Assert.NotEqual(5, completed);
            Assert.Equal(default(int), completed);
        }

        [Fact]
        public void TestThatAwaitCompletes()
        {
            var completes = new BasicCompletes<int>(new Scheduler());

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();

            var completed = completes.Await();

            Assert.Equal(5, completed);
        }

        private class Sender
        {
            private readonly Action<int> callback;
            public Sender(Action<int> callback)
            {
                if (callback != null)
                {
                    this.callback = callback;
                }
            }

            internal void Send(int value)
            {
                callback(value);
            }
        }
    }
}
