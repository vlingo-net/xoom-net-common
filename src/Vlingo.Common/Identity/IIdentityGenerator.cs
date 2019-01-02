// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Security.Cryptography;
using Vlingo.UUID;

namespace Vlingo.Common.Identity
{
    public interface IIdentityGenerator
    {
        Guid Generate();
        Guid Generate(string name);
    }

    internal class NameBasedIdentityGenerator : IIdentityGenerator
    {
        private readonly NameBasedGenerator generator;
        private readonly RandomNumberGenerator random;

        public NameBasedIdentityGenerator()
        {
            generator = new NameBasedGenerator();
            random = new RNGCryptoServiceProvider();
        }

        public Guid Generate()
        {
            var name = Guid.NewGuid().ToString("N");
            return generator.GenerateGuid(name);
        }

        public Guid Generate(string name) => generator.GenerateGuid(name);
    }

    internal class RandomIdentityGenerator : IIdentityGenerator
    {
        private readonly RandomBasedGenerator generator;

        public RandomIdentityGenerator()
        {
            generator = new RandomBasedGenerator();
        }

        public Guid Generate() => generator.GenerateGuid();

        public Guid Generate(string name) => generator.GenerateGuid();
    }

    internal class TimeBasedIdentityGenerator : IIdentityGenerator
    {
        private readonly TimeBasedGenerator generator;

        public TimeBasedIdentityGenerator()
        {
            generator = new TimeBasedGenerator();
        }

        public Guid Generate() => generator.GenerateGuid();

        public Guid Generate(string name) => generator.GenerateGuid();
    }
}
