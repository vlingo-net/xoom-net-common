// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Isopoh.Cryptography.Argon2;

namespace Vlingo.Xoom.Common.Crypto
{
    public class Argon2Hasher : Hasher
    {
        private readonly int _maxDuration;
        private readonly int _memoryCost;
        private readonly int _parallelism;

        public Argon2Hasher(int maxDuration, int memoryCost, int parallelism)
        {
            _maxDuration = maxDuration;
            _memoryCost = memoryCost;
            _parallelism = parallelism;
        }
        
        public override string Hash(string plainSecret) => 
            Argon2.Hash(plainSecret, _maxDuration, _memoryCost, _parallelism);

        public override bool Verify(string plainSecret, string hashedSecret) => 
            Argon2.Verify(hashedSecret, plainSecret, _parallelism);
    }
}