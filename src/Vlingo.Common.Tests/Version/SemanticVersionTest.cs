// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Version;
using Xunit;

namespace Vlingo.Common.Tests.Version
{
    public class SemanticVersionTest
    {
        [Fact]
        public void TestThatVersionEncodesDecodes()
        {
            var semanticVersion_0_0_0 = SemanticVersion.ToValue(0, 0, 0);
            Assert.Equal("0.0.0", SemanticVersion.ToString(semanticVersion_0_0_0));

            var semanticVersion_1_0_0 = SemanticVersion.ToValue(1, 0, 0);
            Assert.Equal("1.0.0", SemanticVersion.ToString(semanticVersion_1_0_0));

            var semanticVersion_0_1_0 = SemanticVersion.ToValue(0, 1, 0);
            Assert.Equal("0.1.0", SemanticVersion.ToString(semanticVersion_0_1_0));

            var semanticVersion_0_0_1 = SemanticVersion.ToValue(0, 0, 1);
            Assert.Equal("0.0.1", SemanticVersion.ToString(semanticVersion_0_0_1));

            var semanticVersion_1_1_0 = SemanticVersion.ToValue(1, 1, 0);
            Assert.Equal("1.1.0", SemanticVersion.ToString(semanticVersion_1_1_0));

            var semanticVersion_1_1_1 = SemanticVersion.ToValue(1, 1, 1);
            Assert.Equal("1.1.1", SemanticVersion.ToString(semanticVersion_1_1_1));

            var semanticVersion_0_1_2 = SemanticVersion.ToValue(0, 1, 2);
            Assert.Equal("0.1.2", SemanticVersion.ToString(semanticVersion_0_1_2));

            var semanticVersion_1_2_3 = SemanticVersion.ToValue(1, 2, 3);
            Assert.Equal("1.2.3", SemanticVersion.ToString(semanticVersion_1_2_3));

            var semanticVersion_129_64_55 = SemanticVersion.ToValue(129, 64, 55);
            Assert.Equal("129.64.55", SemanticVersion.ToString(semanticVersion_129_64_55));

            var semanticVersion_32761_127_127 = SemanticVersion.ToValue(32761, 127, 127);
            Assert.Equal("32761.127.127", SemanticVersion.ToString(semanticVersion_32761_127_127));

            var semanticVersion_32767_255_255 = SemanticVersion.ToValue(32767, 255, 255);
            Assert.Equal("32767.255.255", SemanticVersion.ToString(semanticVersion_32767_255_255));
        }

        [Fact]
        public void TestThatMajorVersionMinBoundsCheck()
        {
            Assert.Throws<ArgumentException>(() => SemanticVersion.ToValue(-1, 1, 1));
        }

        [Fact]
        public void TestThatMajorVersionMaxBoundsCheck()
        {
            Assert.Throws<ArgumentException>(() => SemanticVersion.ToValue(32768, 1, 1));
        }

        [Fact]
        public void TestThatMinorVersionMinBoundsCheck()
        {
            Assert.Throws<ArgumentException>(() => SemanticVersion.ToValue(1, -1, 1));
        }

        [Fact]
        public void TestThatMinorVersionMaxBoundsCheck()
        {
            Assert.Throws<ArgumentException>(() => SemanticVersion.ToValue(1, 256, 1));
        }

        [Fact]
        public void TestThatPatchVersionMinBoundsCheck()
        {
            Assert.Throws<ArgumentException>(() => SemanticVersion.ToValue(1, 1, -1));
        }

        [Fact]
        public void testThatPatchVersionMaxBoundsCheck()
        {
            Assert.Throws<ArgumentException>(() => SemanticVersion.ToValue(1, 1, 256));
        }
    }
}
