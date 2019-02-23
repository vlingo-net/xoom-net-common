// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vlingo.Common.Compiler
{
    public class DynaClassLoader
    {
        internal Type AddDynaClass(string fullyQualifiedClassName, byte[] byteCode)
        {
            var loadedAssembly = Assembly.Load(byteCode);
            return loadedAssembly.GetType(fullyQualifiedClassName);
        }

        internal Type AddDynaClass(string fullyQualifiedClassName, string dllPath)
        {
            var loadedAssembly = Assembly.LoadFrom(dllPath);
            return loadedAssembly.GetType(fullyQualifiedClassName);
        }

        public Type LoadClass(string fullyQualifiedClassName, Type protocolName = null)
        {
            var candidateTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Vlingo"));
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(fullyQualifiedClassName);
                if (type != null)
                {
                    return type;
                }

                if (protocolName != null)
                {
                    var className = fullyQualifiedClassName
                        .Substring(fullyQualifiedClassName.LastIndexOf(".", StringComparison.InvariantCulture) + 1);
                    candidateTypes.AddRange(assembly.ExportedTypes
                        .Where(t => protocolName.IsAssignableFrom(t) && !t.IsInterface && t.Name == className));
                }
            }

            if (candidateTypes.Any())
            {
                // we return the first found as we don't have other means to check what would be the best match
                return candidateTypes[0];
            }

            return null;
        }
    }
}
