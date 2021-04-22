// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Vlingo.Xoom.Common.Serialization;

namespace Vlingo.Xoom.Common.Expressions
{
    public static class ExpressionSerialization
    {
        public static string Serialize<TProtocol>(Expression<Action<TProtocol>> expression)
        {
            var mce = (MethodCallExpression)expression.Body;

            var methodName = mce.Method.Name;

            var args = mce.Arguments.Select(ExpressionExtensions.Evaluate).ToArray();
            var types = args.Select(a => a?.GetType()).ToArray();
            return JsonSerialization.Serialized(new ExpressionSerializationInfo(methodName, args, types));
        }

        public static ExpressionSerializationInfo Deserialize(string serialized)
        {
            var deserialized = JsonSerialization.Deserialized<ExpressionSerializationInfo>(serialized);
            var i = 0;
            foreach (var arg in deserialized.Args)
            {
                if (arg is JObject jobject && deserialized.Types.Length >= i)
                {
                    deserialized.Args[i] = jobject.ToObject(deserialized.Types[i]!);
                }
                
                // special case as underlying serializer converts ints to longs en deserialization
                else if (arg is long longArg && arg.GetType() != deserialized.Types[i])
                {
                    deserialized.Args[i] = int.Parse(longArg.ToString());
                }

                i++;
            }

            return deserialized;
        }
    }
}