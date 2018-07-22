// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Runtime.Loader;

namespace Vlingo.Common.Compiler
{
    public class DynaClassLoader
    {
        private readonly AssemblyLoadContext context;

        public DynaClassLoader(AssemblyLoadContext context)
        {
            this.context = context;
        }

        internal Type AddDynaClass(string fullyQualifiedClassName, byte[] byteCode)
        {
            using(var stream = new MemoryStream(byteCode, false))
            {
                var loadedAssembly = context.LoadFromStream(stream);
                return loadedAssembly.GetType(fullyQualifiedClassName);
            }
        }

        internal Type AddDynaClass(string fullyQualifiedClassName, string dllPath)
        {
            var loadedAssembly = context.LoadFromAssemblyPath(dllPath);
            return loadedAssembly.GetType(fullyQualifiedClassName);
        }

        public Type LoadClass(string fullyQualifiedClassName)
        {
            var assemblies = AppDomain.CurrentDomain?.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                var type = assembly.GetType(fullyQualifiedClassName);
                if(type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
