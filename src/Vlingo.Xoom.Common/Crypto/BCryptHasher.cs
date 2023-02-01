// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Common.Crypto;

public class BCryptHasher : Hasher
{
    public override string Hash(string plainSecret) => BCrypt.Net.BCrypt.HashPassword(plainSecret);

    public override bool Verify(string plainSecret, string hashedSecret) => 
        BCrypt.Net.BCrypt.Verify(plainSecret, hashedSecret);
}