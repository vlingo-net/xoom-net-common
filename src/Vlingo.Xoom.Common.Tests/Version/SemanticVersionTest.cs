// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Version;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Version
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
        public void TestThatStringToValueParses()
        {
            var semanticVersion_1_0_0 = SemanticVersion.ToValue(1, 0, 0);
            var semanticVersion_1_0_0_again = SemanticVersion.ToValue(SemanticVersion.ToString(semanticVersion_1_0_0));
            Assert.Equal(semanticVersion_1_0_0, semanticVersion_1_0_0_again);

            var semanticVersion_0_1_0 = SemanticVersion.ToValue(0, 1, 0);
            var semanticVersion_0_1_0_again = SemanticVersion.ToValue(SemanticVersion.ToString(semanticVersion_0_1_0));
            Assert.Equal(semanticVersion_0_1_0, semanticVersion_0_1_0_again);

            var semanticVersion_0_0_1 = SemanticVersion.ToValue(0, 0, 1);
            var semanticVersion_0_0_1_again = SemanticVersion.ToValue(SemanticVersion.ToString(semanticVersion_0_0_1));
            Assert.Equal(semanticVersion_0_0_1, semanticVersion_0_0_1_again);

            var semanticVersion_1_1_0 = SemanticVersion.ToValue(1, 1, 0);
            var semanticVersion_1_1_0_again = SemanticVersion.ToValue(SemanticVersion.ToString(semanticVersion_1_1_0));
            Assert.Equal(semanticVersion_1_1_0, semanticVersion_1_1_0_again);

            var semanticVersion_1_1_1 = SemanticVersion.ToValue(1, 1, 1);
            var semanticVersion_1_1_1_again = SemanticVersion.ToValue(SemanticVersion.ToString(semanticVersion_1_1_1));
            Assert.Equal(semanticVersion_1_1_1, semanticVersion_1_1_1_again);
        }

        [Fact]
        public void TestVersionCompatibility()
        {
            var version = SemanticVersion.From(1, 0, 0);

            var majorBump = SemanticVersion.From(2, 0, 0);
            Assert.True(majorBump.IsCompatibleWith(version));

            var minorBump = SemanticVersion.From(1, 1, 0);
            Assert.True(minorBump.IsCompatibleWith(version));

            var patchBump = SemanticVersion.From(1, 0, 1);
            Assert.True(patchBump.IsCompatibleWith(version));
        }
        
        [Fact]
        public void TestVersionIncrements()
        {
            var version = SemanticVersion.From(1, 2, 3);

            Assert.Equal(version.NextPatch(), SemanticVersion.From(1, 2, 4));
            Assert.Equal(version.NextMinor(), SemanticVersion.From(1, 3, 0));
            Assert.Equal(version.NextMajor(), SemanticVersion.From(2, 0, 0));
        }

        [Fact]
        public void TestVersionIncompatibility()
        {
            var version = SemanticVersion.From(1, 0, 0);

            var majorTooHigh = SemanticVersion.From(3, 0, 0);
            Assert.False(majorTooHigh.IsCompatibleWith(version));

            var majorBumpMinorTooHigh = SemanticVersion.From(2, 1, 0);
            Assert.False(majorBumpMinorTooHigh.IsCompatibleWith(version));

            var minorTooHigh = SemanticVersion.From(1, 2, 0);
            Assert.False(minorTooHigh.IsCompatibleWith(version));

            var minorBumpPatchTooHigh = SemanticVersion.From(1, 1, 1);
            Assert.False(minorBumpPatchTooHigh.IsCompatibleWith(version));

            var patchTooHigh = SemanticVersion.From(1, 0, 2);
            Assert.False(patchTooHigh.IsCompatibleWith(version));
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
        public void TestThatPatchVersionMaxBoundsCheck()
        {
            Assert.Throws<ArgumentException>(() => SemanticVersion.ToValue(1, 1, 256));
        }
    }
}
