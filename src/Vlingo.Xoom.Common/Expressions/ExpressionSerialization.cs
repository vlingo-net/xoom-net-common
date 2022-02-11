// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
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

namespace Vlingo.Xoom.Common.Expressions;

public static class ExpressionSerialization
{
    public static string? Serialize(LambdaExpression? expression)
    {
        if (expression == null)
        {
            return null;
        }
            
        var mce = (MethodCallExpression)expression.Body;

        var methodName = mce.Method.Name;

        var args = mce.Arguments.Select(ExpressionExtensions.Evaluate).ToArray();
        var argumentTypes = args.Select(a => a?.GetType()).ToArray();
        var parameters = expression.Parameters.Select(p => new ParameterExpressionNode(p.Name, p.Type)).ToArray();
            
        return JsonSerialization.Serialized(new ExpressionSerializationInfo(methodName, parameters, args, argumentTypes));
    }

    public static ExpressionSerializationInfo Deserialize(string serialized)
    {
        var deserialized = JsonSerialization.Deserialized<ExpressionSerializationInfo>(serialized);
        var i = 0;
        foreach (var arg in deserialized.ArgumentValues)
        {
            if (arg is JObject jobject && deserialized.ArgumentTypes.Length >= i)
            {
                deserialized.ArgumentValues[i] = jobject.ToObject(deserialized.ArgumentTypes[i]!);
            }
                
            // special case as underlying serializer converts ints to longs en deserialization
            else if (arg is long longArg && arg.GetType() != deserialized.ArgumentTypes[i])
            {
                deserialized.ArgumentValues[i] = int.Parse(longArg.ToString());
            }

            i++;
        }

        return deserialized;
    }
}
    
public class ParameterExpressionNode
{
    public string Name { get; }
    public Type Type { get; }

    public ParameterExpressionNode(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}