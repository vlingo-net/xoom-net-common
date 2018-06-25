// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.Loader;

namespace Vlingo.Common.Compiler
{
    public static class AssemblyContextHelper
    {
        public static AssemblyLoadContext SystemDefaultContext => AssemblyLoadContext.Default;

        public static AssemblyLoadContext GetAssemblyLoadContext(this Type type)
            => AssemblyLoadContext.GetLoadContext(type.Assembly);
    }
}
