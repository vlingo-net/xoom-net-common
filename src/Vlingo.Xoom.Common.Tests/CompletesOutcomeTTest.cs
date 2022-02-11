// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.Xoom.Common.Tests;

public class CompletesOutcomeTTest
{
    [Fact]
    public void TestWithTheSameTypeT()
    {
        var out0 = CompletesOutcomeT<Exception, int>.Of(AddsOne(1));
        var out1 = out0.AndThenTo(i => CompletesOutcomeT<Exception, int>.Of(AddsTwo(i)));
        out1.Value.AndThenConsume(o => Assert.Equal(4L, o.GetOrNull()));
    }
        
    [Fact]
    public void TestWithDifferentTypeT()
    {
        var out0 = CompletesOutcomeT<Exception, string>.Of(ToString(1));
        var out1 = out0.AndThenTo(s => CompletesOutcomeT<Exception, float>.Of(ToFloat(s)));
        out1.Value.AndThenConsume(o => Assert.Equal(1F, (float)o.GetOrNull(), 0));
    }
        
    [Fact]
    public void TestFailure() 
        => CompletesOutcomeT<ArithmeticException, float>.Of(DivZero(42)).Value.AndThenConsume(o => Assert.True(o is Failure<ArithmeticException, float>));

    private ICompletes<IOutcome<Exception, int>> AddsOne(int x) => Completes.WithSuccess(Success.Of<Exception, int>(x + 1));

    private ICompletes<IOutcome<Exception, int>> AddsTwo(int x) => Completes.WithSuccess(Success.Of<Exception, int>(x + 2));

    private ICompletes<IOutcome<Exception, string>> ToString(int i) => Completes.WithSuccess(Success.Of<Exception, string>(i.ToString()));

    private ICompletes<IOutcome<Exception, float>> ToFloat(string s) => Completes.WithSuccess(Success.Of<Exception, float>(float.Parse(s)));
        
    private ICompletes<IOutcome<ArithmeticException, float>> DivZero(float x)
    {
        IOutcome<ArithmeticException, float> outcome;
        var value = x / 0;
            
        if (float.IsPositiveInfinity(value))
        {
            outcome = Failure.Of<ArithmeticException, float>(new ArithmeticException("division by zero"));
        }
        else
        {
            outcome = Success.Of<ArithmeticException, float>(value);
        }
            
        return Completes.WithSuccess(outcome);
    }
}