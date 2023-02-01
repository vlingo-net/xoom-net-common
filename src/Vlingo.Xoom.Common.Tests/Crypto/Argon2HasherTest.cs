// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Crypto;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Crypto;

public class Argon2HasherTest
{
    [Fact]
    public void TestThatHashVerifiesSimple()
    {
        var secret = "secret";
        var hasher = new Argon2Hasher(2, 65535, 1);
        var hashed = hasher.Hash(secret);
        Assert.True(hasher.Verify(secret, hashed));
    }
        
    [Fact]
    public void TestThatHashVerifiesComplext()
    {
        var secret = "Thi$ isAM0re C*mple+ S3CR37";
        var hasher = new Argon2Hasher(10, 65535, 1);
        var hashed = hasher.Hash(secret);
        Assert.True(hasher.Verify(secret, hashed));
    }

    [Fact]
    public void TestThatHashVerifiesComplextGreaterTiming()
    {
        var secret = "(Thi$) isAn Ev0nM0re c*mple+ S3CR37 --!.";
        var hasher = new Argon2Hasher(20, 65535, 1);
        var hashed = hasher.Hash(secret);
        Assert.True(hasher.Verify(secret, hashed));
    }
}