// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;
using Vlingo.Xoom.Common.Expressions;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Expressions
{
    public class ExpressionSerializationTest
    {
        [Fact]
        public void TestThatSimpleExpressionInvokes()
        {
            var actor = new ActorTest();
            var m = "Test";
            Expression<Action<int>> expression = i => actor.DoSomething(m, i);
            var serialized = ExpressionSerialization.Serialize(expression);
            
            var serializationInfo = ExpressionSerialization.Deserialize(serialized);
            var consumer = ExpressionExtensions.CreateDelegate<IParameterlessInterface, int>(actor, serializationInfo);
            consumer(actor, 100);
            
            Assert.Equal(100, actor.Count);
            Assert.Equal(m, actor.Message);
        }
        
        [Fact]
        public void TestThatParameterlessExpressionInvokes()
        {
            Expression<Action<IParameterlessInterface>> expression = a => a.DoSomething();
            var serialized = ExpressionSerialization.Serialize(expression);
            
            var actor = new ActorTest();
            var serializationInfo = ExpressionSerialization.Deserialize(serialized);
            var consumer = ExpressionExtensions.CreateDelegate<IParameterlessInterface>(actor, serializationInfo);
            consumer(actor);
            Assert.True(actor.WasRun);
        }
        
        [Fact]
        public void TestThatSimpleParameterExpressionInvokes()
        {
            var message = "hello";
            var i = 10;
            Expression<Action<ISimpleParametersInterface>> expression = a => a.DoSomething(message, i);
            var serialized = ExpressionSerialization.Serialize(expression);
            
            var actor = new ActorTest();
            var serializationInfo = ExpressionSerialization.Deserialize(serialized);
            var consumer = ExpressionExtensions.CreateDelegate<ISimpleParametersInterface>(actor, serializationInfo);
            consumer(actor);
            Assert.Equal(i, actor.Count);
            Assert.Equal(message, actor.Message);
        }
        
        [Fact]
        public void TestThatComplexParameterExpressionInvokes()
        {
            var i = 10;
            var complexParameters = new ComplexParameters
            {
                Int = 1,
                Message = "Hello",
                InnerParameters = new ComplexParameters
                {
                    Int = 2,
                    Message = "World"
                }
            };
            Expression<Action<IComplexParameterInterface>> expression = a => a.DoSomething(i, complexParameters);
            var serialized = ExpressionSerialization.Serialize(expression);

            var actor = new ActorTest();
            var serializationInfo = ExpressionSerialization.Deserialize(serialized);
            var consumer = ExpressionExtensions.CreateDelegate<IComplexParameterInterface>(actor, serializationInfo);
            consumer(actor);
            Assert.Equal(i, actor.Count);
            Assert.Equal(complexParameters, actor.Parameters);
        }
    }

    public interface IParameterlessInterface
    {
        void DoSomething();
    }

    public interface ISimpleParametersInterface
    {
        void DoSomething(string message, int count);
    }

    public interface IComplexParameterInterface
    {
        void DoSomething(int count, ComplexParameters parameters);
    }
    
    public class ActorTest : IParameterlessInterface, ISimpleParametersInterface, IComplexParameterInterface
    {
        public bool WasRun { get; private set; }
        public string Message { get; private set; }
        public int Count { get; private set; }
        public ComplexParameters Parameters { get; private set; }
        
        public void DoSomething() => WasRun = true;

        public void DoSomething(string message, int count)
        {
            Message = message;
            Count = count;
        }

        public void DoSomething(int count, ComplexParameters parameters)
        {
            Count = count;
            Parameters = parameters;
        }
    }

    [Serializable]
    public class ComplexParameters
    {
        public int Int { get; set; }
        
        public string Message { get; set; }
        
        public ComplexParameters InnerParameters { get; set; }
        
        protected bool Equals(ComplexParameters other) => 
            Int == other.Int 
            && Message == other.Message
            && Equals(InnerParameters, other.InnerParameters);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ComplexParameters) obj);
        }

        public override int GetHashCode() => HashCode.Combine(Int, Message, InnerParameters);
    }
}