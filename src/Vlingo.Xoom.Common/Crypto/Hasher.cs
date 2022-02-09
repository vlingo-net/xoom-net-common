// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
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
                    var cryptoArgon2MaxDuration = int.Parse(properties.GetProperty("crypto.argon2.max.duration", "10")!);
                    var cryptoArgon2MemoryCost = int.Parse(properties.GetProperty("crypto.argon2.memory.cost", "65536")!);
                    var cryptoArgon2Parallelism = int.Parse(properties.GetProperty("crypto.argon2.parallelism", "1")!);
                    return new Argon2Hasher(cryptoArgon2MaxDuration, cryptoArgon2MemoryCost, cryptoArgon2Parallelism);
                case "scrypt":
                    var cryptoScryptNCostFactor = int.Parse(properties.GetProperty("crypto.scrypt.N.cost.factor", "16384")!);
                    var cryptoScryptRBlocksize = int.Parse(properties.GetProperty("crypto.scrypt.r.blocksize", "8")!);
                    var cryptoScryptPParallelization = int.Parse(properties.GetProperty("crypto.scrypt.p.parallelization", "1")!);
                    return new SCryptHasher(cryptoScryptNCostFactor, cryptoScryptRBlocksize, cryptoScryptPParallelization);
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