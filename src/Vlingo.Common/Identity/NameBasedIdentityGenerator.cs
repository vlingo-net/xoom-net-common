// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.UUID;

namespace Vlingo.Common.Identity
{
    internal class NameBasedIdentityGenerator : IIdentityGenerator
    {
        private readonly NameBasedGenerator generator;

        public NameBasedIdentityGenerator()
        {
            generator = new NameBasedGenerator();
        }

        public Guid Generate()
        {
            var name = Guid.NewGuid().ToString("N");
            return generator.GenerateGuid(name);
        }

        public Guid Generate(string name) => generator.GenerateGuid(name);
    }
}