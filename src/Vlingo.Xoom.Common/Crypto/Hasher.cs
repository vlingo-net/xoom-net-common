// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Common.Crypto
{
    public abstract class Hasher
    {
        public static Hasher DefaultHasher(ConfigurationProperties properties)
        {
            switch (properties.GetProperty("crypto.type", "(unknown)"))
            {
                case "argon2":
                    throw new NotImplementedException("Argon2 is not implemented");
                case "scrypt":
                    throw new NotImplementedException("scrypt is not implemented");
                case "bcrypt":
                    throw new NotImplementedException("bcrypt is not implemented");
                default:
                    throw new NotImplementedException("Crypto type is not defined.");
            }
        }

        public abstract string Hash(string plainSecret);
        public abstract bool Verify(string plainSecret, string hashedSecret);
    }
}