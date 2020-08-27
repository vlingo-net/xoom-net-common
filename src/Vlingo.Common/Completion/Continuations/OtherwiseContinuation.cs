// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Continuations
{
    internal class OtherwiseContinuation<TAntecedentResult, TResult> : BasicCompletes<TResult>
    {
        private readonly BasicCompletes<TAntecedentResult> _antecedent;

        internal OtherwiseContinuation(BasicCompletes parent, BasicCompletes<TAntecedentResult> antecedent, Delegate function) :
            base(function, parent) =>
            _antecedent = antecedent;

        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (HasException.Get())
            {
                return;
            }
            
            if (Action is Action invokableAction)
            {
                invokableAction();
                return;
            }
            
            if (Action is Action<TResult> invokableActionInput)
            {
                if (completedCompletes is AndThenContinuation<TResult, TResult> andThenContinuation)
                {
                    invokableActionInput(andThenContinuation.FailedOutcomeValue.Get());
                    OutcomeKnown.Set();
                    return;   
                }
            }
            
            if (Action is Func<ICompletes<TAntecedentResult>, TResult> funcCompletes)
            {
                OutcomeValue.Set(funcCompletes(_antecedent));
                OutcomeKnown.Set();
                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                if (completedCompletes is AndThenContinuation<TResult, TAntecedentResult> andThenContinuation)
                {
                    OutcomeValue.Set(function(andThenContinuation.FailedOutcomeValue.Get()));
                    OutcomeKnown.Set();
                    return;   
                }

                if (completedCompletes is BasicCompletes<TAntecedentResult> otherwiseContinuation)
                {
                    OutcomeValue.Set(function(otherwiseContinuation.FailedOutcomeValue.Get()));
                    OutcomeKnown.Set();
                    return;
                }
            }
            
            base.InnerInvoke(completedCompletes);

            throw new InvalidCastException("Cannot run 'Otherwise' function. Make sure that expecting type is the same as failedOutcomeValue type");
        }
    }
}