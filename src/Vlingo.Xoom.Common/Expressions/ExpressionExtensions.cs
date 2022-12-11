// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Vlingo.Xoom.Common.Expressions;

public static class ExpressionExtensions
{
    public static Action<TProtocol> CreateDelegate<TProtocol>(object actor, ExpressionSerializationInfo info)
    {
        var mi = actor.GetType().GetMethod(info.MethodName, info.ArgumentTypes!);
        
        if (mi != null)
        {
            var dlg = CreateDelegate(mi, actor);
            Action<TProtocol> consumer = _ => dlg.DynamicInvoke(info.ArgumentValues);
            return consumer;   
        }

        throw new InvalidOperationException($"Cannot Create a delegate method for MethodName={info.MethodName}");
    }
        
    public static Action<TProtocol, TParam> CreateDelegate<TProtocol, TParam>(object actor, ExpressionSerializationInfo info)
    {
        var mi = actor.GetType().GetMethod(info.MethodName, info.FlattenTypes());
        
        if (mi != null)
        {
            var dlg = CreateDelegate(mi, actor);
            var args = info.ArgumentValues.Where(a => !(a is ParameterExpressionNode)).ToArray();
            Action<TProtocol, TParam> consumer = (_, tparam) => dlg.DynamicInvoke(args.Concat(new object?[] {tparam}).ToArray());
            return consumer;   
        }

        throw new InvalidOperationException($"Cannot Create a delegate method for MethodName={info.MethodName}");
    }
        
    public static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
    {
        Func<Type[], Type> getType;
        var isAction = methodInfo.ReturnType == typeof(void);
        var types = methodInfo.GetParameters().Select(p => p.ParameterType);

        if (isAction)
        {
            getType = Expression.GetActionType;
        }
        else
        {
            getType = Expression.GetFuncType;
            types = types.Concat(new[] { methodInfo.ReturnType });
        }

        if (methodInfo.IsStatic)
        {
            return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
        }

        return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
    }
        
    public static object? Evaluate(this Expression? expr)
    {
        switch (expr?.NodeType)
        {
            case ExpressionType.Constant:
                return ((ConstantExpression)expr).Value;
            case ExpressionType.MemberAccess:
                var me = (MemberExpression)expr;
                var target = Evaluate(me.Expression);
                    
                switch (me.Member.MemberType)
                {
                    case MemberTypes.Field:
                        return ((FieldInfo)me.Member).GetValue(target);
                    case MemberTypes.Property:
                        return ((PropertyInfo)me.Member).GetValue(target, null);
                    default:
                        throw new NotSupportedException(me.Member.MemberType.ToString());
                }
            case ExpressionType.New:
                return ((NewExpression)expr).Constructor?.Invoke(((NewExpression)expr).Arguments.Select(Evaluate).ToArray());
            case ExpressionType.Parameter:
                var parameter = (ParameterExpression)expr;
                return new ParameterExpressionNode(parameter.Name, parameter.Type);
            default:
                throw new NotSupportedException(expr?.NodeType.ToString());
        }
    }

    public static Expression<Func<T>> Curry<T, TParameter>(
        this Expression<Func<TParameter, T>> expressionToCurry,
        TParameter valueToProvide)
    {
        var newExpression = expressionToCurry.Body as NewExpression;
        var arguments = newExpression?.Arguments;
        var argumentValues = new List<ConstantExpression>();
        foreach (var argument in arguments!)
        {
            var value = argument.Evaluate();
            if (value is ParameterExpressionNode parameterExpressionNode)
            {
                if (parameterExpressionNode.Type == valueToProvide!.GetType())
                {
                    argumentValues.Add(Expression.Constant(valueToProvide));
                }
            }
            else if (value != null)
            {
                argumentValues.Add(Expression.Constant(value));
            }
        }
        var constructor = newExpression?.Constructor;
        var updatedExpression = Expression.New(constructor!, argumentValues);
        var lambda = Expression.Lambda<Func<T>>(updatedExpression);
        return lambda;
    }
}