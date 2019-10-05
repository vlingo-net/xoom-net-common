// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
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
        private readonly BasicCompletes<TAntecedentResult> antecedent;

        internal OtherwiseContinuation(BasicCompletes<TAntecedentResult> antecedent, Delegate function) :
            base(function) => this.antecedent = antecedent;

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
                    return;   
                }
            }
            
            if (Action is Func<ICompletes<TAntecedentResult>, TResult> funcCompletes)
            {
                Result = funcCompletes(antecedent);
                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                if (completedCompletes is AndThenContinuation<TResult, TAntecedentResult> andThenContinuation)
                {
                    Result = function(andThenContinuation.FailedOutcomeValue.Get());
                    return;   
                }
            }
            
            base.InnerInvoke(completedCompletes);

            throw new InvalidCastException("Cannot run 'Otherwise' function. Make sure that expecting type is the same as failedOutcomeValue type");
        }

        internal override BasicCompletes Antecedent => antecedent;

        internal override Exception Exception => antecedent.Exception;
        
        internal override void RegisterContinuation(CompletesContinuation continuation) => antecedent.RegisterContinuation(continuation);

        internal override void RegisterFailureContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterFailureContiuation(continuationCompletes);
        
        internal override void RegisterExceptionContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterExceptionContiuation(continuationCompletes);
    }
}