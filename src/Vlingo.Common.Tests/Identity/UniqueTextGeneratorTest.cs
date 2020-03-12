// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Common.Identity;
using Xunit;

namespace Vlingo.Common.Tests.Identity
{
    public class UniqueTextGeneratorTest
    {
        [Fact]
        public void TestThatUniqueTextGenerates()
        {
            var length = 10;
            var cycles = 100;
            var maximum = 10_000;
            var total = cycles * maximum;

            var all = new HashSet<string>(total);

            var generator = new UniqueTextGenerator();

            for (var count = 0; count < cycles; ++count)
            {
                for (var idx = 0; idx < maximum; ++idx)
                {
                    var generated = generator.Generate(length);
                    Assert.Equal(length, generated.Length);
                    Assert.True(all.Add(generated));
                }
            }

            Assert.Equal(total, all.Count);
        }
    }
}
