// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Scrypt;

namespace Vlingo.Xoom.Common.Crypto
{
    public class SCryptHasher : Hasher
    {
        private ScryptEncoder encoder;
        
        public SCryptHasher(int costFactor, int blocksize, int parallelization) => 
            encoder = new ScryptEncoder(costFactor, blocksize, parallelization);

        public override string Hash(string plainSecret) => 
            encoder.Encode(plainSecret);

        public override bool Verify(string plainSecret, string hashedSecret) => 
            encoder.Compare(plainSecret, hashedSecret);
    }
}