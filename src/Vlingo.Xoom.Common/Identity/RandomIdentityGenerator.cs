// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.UUID;

namespace Vlingo.Xoom.Common.Identity;

internal class RandomIdentityGenerator : IIdentityGenerator
{
    private readonly RandomBasedGenerator _generator = new RandomBasedGenerator();

    public Guid Generate() => _generator.GenerateGuid();

    public Guid Generate(string name) => _generator.GenerateGuid();
}