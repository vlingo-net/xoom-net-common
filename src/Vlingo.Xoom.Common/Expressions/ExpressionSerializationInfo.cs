// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Xoom.Common.Expressions;

public class ExpressionSerializationInfo
{
    public string MethodName { get; }
    public ParameterExpressionNode[] Parameters { get; }
    public object?[] ArgumentValues { get; }
    public Type?[] ArgumentTypes { get; }

    public ExpressionSerializationInfo(
        string methodName,
        ParameterExpressionNode[] parameters,
        object?[] argumentValues,
        Type?[] argumentTypes)
    {
        MethodName = methodName;
        Parameters = parameters;
        ArgumentValues = argumentValues;
        ArgumentTypes = argumentTypes;
    }

    public Type[] FlattenTypes()
    {
        var flatTypes = new List<Type>(ArgumentTypes.Length);
        for (var i = 0; i < ArgumentTypes.Length; i++)
        {
            if (typeof(ParameterExpressionNode) == ArgumentTypes[i])
            {
                flatTypes.Add(((ParameterExpressionNode) ArgumentValues[i]!).Type);
            }
            else
            {
                flatTypes.Add(ArgumentTypes[i]!);
            }
        }

        return flatTypes.ToArray();
    }
}