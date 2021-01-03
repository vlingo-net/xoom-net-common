// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

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
    }
}
