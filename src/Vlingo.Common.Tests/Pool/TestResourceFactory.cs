// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Pool;

namespace Vlingo.Common.Tests.Pool
{
    public class TestResourceFactory : IResourceFactory<int, Nothing>
    {
        private static readonly Random Random = new Random();

        public int Create(Nothing arguments) => Random.Next();

        public Nothing DefaultArguments => Nothing.AtAll;

        public int Reset(int resource, Nothing arguments) => resource;

        public void Destroy(int resource)
        {
        }
    }
}