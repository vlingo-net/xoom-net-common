// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
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
        private readonly AtomicReference<BasicCompletes<TAntecedentResult>> antecedent = new AtomicReference<BasicCompletes<TAntecedentResult>>(default);

        internal AndThenContinuation(BasicCompletes parent, BasicCompletes<TAntecedentResult> antecedent, Delegate function) : this(parent, antecedent, Optional.Empty<TResult>(), function)
        {
        }
        
        internal AndThenContinuation(BasicCompletes parent, BasicCompletes<TAntecedentResult> antecedent, Optional<TResult> failedOutcomeValue, Delegate function) : base(function)
        {
            Parent = parent;
            this.antecedent.Set(antecedent);
            FailedOutcomeValue = failedOutcomeValue;
        }

        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (HasFailedValue.Get())
            {
                return;
            }
            
            base.InnerInvoke(completedCompletes);

            if (Action is Func<TAntecedentResult, ICompletes<TResult>> funcCompletes)
            {
                CompletesResult = funcCompletes(antecedent.Get().Outcome);
                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                Result = function(antecedent.Get().Outcome);
                TransformedResult = Result;
            }
        }

        /*internal override void HandleFailure()
        {
            Result = FailedOutcomeValue.Get();
            base.HandleFailure();
            antecedent.Get().HandleFailure();
        }

        internal override void UpdateFailure(BasicCompletes previousContinuation)
        {
            if (previousContinuation.HasFailedValue.Get())
            {
                HasFailedValue.Set(true);
                return;
            }
            
            if (previousContinuation is BasicCompletes<TAntecedentResult> completes)
            {
                if (completes.CompletesResult != null)
                {
                    if (completes.CompletesResult is BasicCompletes<TAntecedentResult> basicCompletes)
                    {
                        HasFailedValue.Set(basicCompletes.Outcome.Equals(FailedOutcomeValue.Get()));
                    }
                }
                else
                {
                    HasFailedValue.Set(HasFailedValue.Get() || completes.Outcome.Equals(FailedOutcomeValue.Get()));   
                }
            }
        }*/
    }
}