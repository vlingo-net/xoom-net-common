// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;

namespace Vlingo.Xoom.Common.Compiler;

public static class DynaNaming
{
    public static string ClassNameFor<T>(string postfix, bool forTypeLookup = false)
        => ClassNameFor(typeof(T), postfix, forTypeLookup);

    public static string ClassNameFor(Type type, string postfix, bool forTypeLookup = false)
    {
        var className = type.Name;

        if (type.IsInterface && type.Name.StartsWith("I"))
        {
            className = className.Substring(1);
        }
        if (type.IsGenericType)
        {
            className = className.Substring(0, className.IndexOf('`'));
        }

        var genericTypeParams = string.Empty;
        if (type.IsGenericType)
        {
            var numGenericParams = type.GetGenericArguments().Length;
            if (forTypeLookup)
            {
                genericTypeParams = $"`{numGenericParams}";
            }
            else
            {
                var genericDefinition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
                var typeListString = string.Join(", ", genericDefinition.GetGenericArguments().Select(x => x.FullName ?? x.Name));
                genericTypeParams = $"<{typeListString}>";
            }
        }

        return $"{className}{postfix}{genericTypeParams}";
    }

    public static string FullyQualifiedClassNameFor<T>(string postfix, bool forTypeLookup = false)
        => FullyQualifiedClassNameFor(typeof(T), postfix, forTypeLookup);

    public static string FullyQualifiedClassNameFor(Type type, string postfix, bool forTypeLookup = false)
    {
        if (string.IsNullOrWhiteSpace(type.Namespace))
        {
            return ClassNameFor(type, postfix, forTypeLookup);
        }

        return $"{type.Namespace}.{ClassNameFor(type, postfix, forTypeLookup)}";
    }
}