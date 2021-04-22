// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Vlingo.Xoom.Common.Expressions
{
    public static class ExpressionExtensions
    {
        public static Action<TProtocol> CreateDelegate<TProtocol>(object actor, ExpressionSerializationInfo info)
        {
            var mi = actor.GetType().GetMethod(info.MethodName, info.Types!);
        
            if (mi != null)
            {
                var dlg = CreateDelegate(mi, actor);
                Action<TProtocol> consumer = a => dlg.DynamicInvoke(info.Args);
                return consumer;   
            }

            throw new InvalidOperationException($"Cannot Create a delegate method for MethodName={info.MethodName}");
        }
        
        public static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType == typeof(void);
            var types = methodInfo!.GetParameters().Select(p => p.ParameterType);

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
        
        public static object? Evaluate(this Expression expr)
        {
            switch (expr.NodeType)
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
                    return ((NewExpression)expr).Constructor
                        .Invoke(((NewExpression)expr).Arguments.Select(Evaluate).ToArray());
                default:
                    throw new NotSupportedException(expr.NodeType.ToString());
            }
        }
    }
}