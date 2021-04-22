// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Identity
{
    public enum IdentityGeneratorType
    {
        TimeBased,
        NameBased,
        Random
    }

    public static class IdentityGeneratorTypeExtension
    {
        public static IIdentityGenerator Generator(this IdentityGeneratorType type)
        {
            switch (type)
            {
                case IdentityGeneratorType.NameBased:
                    return new NameBasedIdentityGenerator();
                case IdentityGeneratorType.Random:
                    return new RandomIdentityGenerator();
                case IdentityGeneratorType.TimeBased:
                    return new TimeBasedIdentityGenerator();
                default:
                    throw new ArgumentException($"Cannot find a UUID generatator for type {Enum.GetName(typeof(IdentityGeneratorType), type)}");
            }
        }
    }
}
