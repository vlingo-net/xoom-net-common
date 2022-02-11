// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Common.Completion.Continuations;

internal class OtherwiseContinuation<TAntecedentResult, TResult> : BasicCompletes<TResult>
{
    private readonly BasicCompletes<TAntecedentResult> _antecedent;

    internal OtherwiseContinuation(BasicCompletes parent, BasicCompletes<TAntecedentResult> antecedent, Delegate function) :
        base(function, parent) =>
        _antecedent = antecedent;

    internal override bool InnerInvoke(BasicCompletes completedCompletes)
    {
        if (HasException.Get())
        {
            return false;
        }
            
        if (Action is Action invokableAction)
        {
            invokableAction();
            return true;
        }
            
        if (Action is Action<TResult> invokableActionInput)
        {
            if (completedCompletes is AndThenContinuation<TResult, TResult> andThenContinuation)
            {
                invokableActionInput(andThenContinuation.FailedOutcomeValue.Get());
                OutcomeKnown.Set();
                return true;   
            }
        }
            
        if (Action is Func<ICompletes<TAntecedentResult>, TResult> funcCompletes)
        {
            OutcomeValue.Set(funcCompletes(_antecedent));
            OutcomeKnown.Set();
            return true;
        }

        if (Action is Func<TAntecedentResult, TResult> function)
        {
            if (completedCompletes is AndThenContinuation<TResult, TAntecedentResult> andThenContinuation)
            {
                OutcomeValue.Set(function(andThenContinuation.FailedOutcomeValue.Get()));
                return true;   
            }

            if (completedCompletes is BasicCompletes<TAntecedentResult> otherwiseContinuation)
            {
                OutcomeValue.Set(function(otherwiseContinuation.FailedOutcomeValue.Get()));
                OutcomeKnown.Set();
                return true;
            }
        }
            
        base.InnerInvoke(completedCompletes);

        throw new InvalidCastException("Cannot run 'Otherwise' function. Make sure that expecting type is the same as failedOutcomeValue type");
    }
}