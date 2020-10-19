// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Continuations
{
    internal class AndThenContinuation<TAntecedentResult, TResult> : BasicCompletes<TResult>
    {
        private readonly AtomicReference<BasicCompletes<TAntecedentResult>> _antecedent = new AtomicReference<BasicCompletes<TAntecedentResult>>(default);

        internal AndThenContinuation(BasicCompletes? parent, BasicCompletes<TAntecedentResult> antecedent, Delegate function)
            : this(parent, antecedent, Optional.Empty<TResult>(), function)
        {
        }
        
        internal AndThenContinuation(BasicCompletes? parent, BasicCompletes<TAntecedentResult> antecedent, Optional<TResult> failedOutcomeValue, Delegate function) : base(function, parent)
        {
            _antecedent.Set(antecedent);
            FailedOutcomeValue = failedOutcomeValue;
        }

        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (HasFailedValue.Get())
            {
                return;
            }

            if (Action is Func<TAntecedentResult, ICompletes<TResult>> funcCompletes)
            {
                var innerCompletes = funcCompletes(_antecedent.Get()!.Outcome);

                if (innerCompletes.HasOutcome) // it's already computed
                {
                    OutcomeValue.Set(innerCompletes.Outcome);
                    TransformedResult = Outcome;
                }
                else // otherwise continuation has to be scheduled
                {
                    innerCompletes.AndThenConsume(outcome =>
                    {
                        OutcomeValue.Set(outcome);
                        TransformedResult = outcome;
                        Parent?.OnResultAvailable(this);
                    });   
                }

                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                OutcomeValue.Set(function(_antecedent.Get()!.Outcome));
                TransformedResult = OutcomeValue.Get();
                OutcomeKnown.Set();
            }
            
            base.InnerInvoke(completedCompletes);
        }

        internal override void UpdateFailure(BasicCompletes previousContinuation)
        {
            if (previousContinuation.HasFailedValue.Get())
            {
                HasFailedValue.Set(true);
                return;
            }
            
            if (previousContinuation is BasicCompletes<TAntecedentResult> completes && completes.HasOutcome)
            {
                HasFailedValue.Set(HasFailedValue.Get() || completes.Outcome!.Equals(FailedOutcomeValue.Get()));
            }
        }
    }
}