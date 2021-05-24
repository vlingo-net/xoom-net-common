// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Text;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;

namespace Vlingo.Xoom.Common.Crypto
{
    public class Argon2Hasher : Hasher
    {
        private readonly Argon2 _argon2;
        private readonly Argon2Config _argon2Config;

        public Argon2Hasher(int maxDuration, int memoryCost, int parallelism)
        {
            _argon2Config = new Argon2Config()
            {
                TimeCost = maxDuration,
                MemoryCost = memoryCost,
                Lanes = parallelism
            };
            _argon2 = new Argon2(_argon2Config);
        }
        
        public override string Hash(string plainSecret)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(plainSecret);
            _argon2Config.Password = passwordBytes;
            using var hashA = _argon2.Hash();
            return _argon2Config.EncodeString(hashA.Buffer);
        }

        public override bool Verify(string plainSecret, string hashedSecret)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(plainSecret);
            _argon2Config.Password = passwordBytes;
            SecureArray<byte>? hashB = null;
            try
            {
                if (_argon2Config.DecodeString(hashedSecret, out hashB) && hashB != null)
                {
                    using var hashToVerify = _argon2.Hash();
                    if (Argon2.FixedTimeEquals(hashB, hashToVerify))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                hashB?.Dispose();
            }

            return false;
        }
    }
}