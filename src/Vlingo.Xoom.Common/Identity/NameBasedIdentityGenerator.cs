// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.UUID;

namespace Vlingo.Xoom.Common.Identity;

internal class NameBasedIdentityGenerator : IIdentityGenerator
{
    private readonly NameBasedGenerator _generator = new NameBasedGenerator();

    public Guid Generate()
    {
        var name = Guid.NewGuid().ToString("N");
        return _generator.GenerateGuid(name);
    }

    public Guid Generate(string name) => _generator.GenerateGuid(name);
}