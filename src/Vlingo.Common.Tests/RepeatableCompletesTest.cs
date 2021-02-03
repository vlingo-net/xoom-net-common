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
    public class RepeatableCompletesTest
    {
        [Fact]
        public void TestThatCompletesRepeats()
        {
            var andThenValue = -1;

            var completes = new RepeatableCompletes<int>(0);

            completes
                .AndThen(val => val * 2)
                .AndThen(val => andThenValue = val)
                .Repeat();

            completes.With(5);
            Assert.Equal(10, andThenValue);

            completes.With(10);
            Assert.Equal(20, andThenValue);

            completes.With(21);
            Assert.Equal(42, andThenValue);
        }
        
        [Fact]
        public void TestThatCompletesRepeatsAfterFailure()
        {
            var andThenValue = -1;

            var completes = new RepeatableCompletes<int>(0);

            completes
                .AndThen(value =>
                {
                    if (value < 10) throw new InvalidOperationException();
                    return value;
                })
                .AndThen(val => val * 2)
                .AndThen(val => andThenValue = val)
                .Repeat();

            completes.With(5);
            completes.Await();
            Assert.True(completes.HasFailed);

            completes.With(10);
            var outcome20 = completes.Await();
            Assert.Equal(20, outcome20);
            Assert.Equal(20, andThenValue);
        }
        
        [Fact]
        public void TestThatCompletesRepeatsAfterTimeout()
        {
            var andThenValue = -1;

            var completes = new RepeatableCompletes<int>(0);

            completes
                .AndThen(1, val => val * 2)
                .AndThen(val => andThenValue = val)
                .Repeat();

            var thread = new Thread(() =>
            {
                Thread.Sleep(10000);
                completes.With(5);
            });
            thread.Start();
           
            completes.Await(TimeSpan.FromMilliseconds(10));
            Assert.True(completes.HasFailed);

            completes.With(10);
            var outcome20 = completes.Await();
            Assert.Equal(20, outcome20);
            Assert.Equal(20, andThenValue);
        }
    }
}
