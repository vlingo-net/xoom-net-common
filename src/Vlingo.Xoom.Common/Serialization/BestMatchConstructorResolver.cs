// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Vlingo.Xoom.Common.Serialization
{
    public class BestMatchConstructorResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var c = base.CreateObjectContract(objectType);
            if (!IsCustomReference(objectType)) return c;
            var constructors =
                objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderBy(e => e.GetParameters().Length).ToList();
            var mostSpecific = constructors.LastOrDefault();
            if (mostSpecific != null)
            {
                c.OverrideCreator = CreateParameterizedConstructor(mostSpecific);
                var paramCtors = CreateConstructorParameters(mostSpecific, c.Properties);
                foreach (var paramCtor in paramCtors)
                {
                    if (!c.CreatorParameters.Contains(paramCtor.PropertyName!))
                    {
                        c.CreatorParameters.Add(paramCtor);
                    }
                }
            }

            return c;
        }
        
        protected virtual bool IsCustomReference(Type objectType) =>
            !objectType.IsValueType
            && !objectType.IsPrimitive
            && !objectType.IsEnum
            && !string.IsNullOrEmpty(objectType.Namespace)
            && !objectType.Namespace.StartsWith("System.");
        
        private ObjectConstructor<object> CreateParameterizedConstructor(MethodBase? method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            
            var c = method as ConstructorInfo;
            if (c != null)
            {
                return a => c.Invoke(a);
            }
            
            return a => method.Invoke(null, a)!;
        }
    }
}