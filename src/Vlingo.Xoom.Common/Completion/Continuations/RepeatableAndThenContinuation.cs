// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Common.Completion.Continuations
{
    internal class RepeatableAndThenContinuation<TAntecedentResult, TResult> : RepeatableCompletes<TResult>
    {
        private readonly AtomicReference<RepeatableCompletes<TAntecedentResult>> _antecedent = new AtomicReference<RepeatableCompletes<TAntecedentResult>>(default);

        internal RepeatableAndThenContinuation(BasicCompletes parent, RepeatableCompletes<TAntecedentResult> antecedent, Optional<TResult> failedOutcomeValue, Delegate function) : base(function, parent)
        {
            _antecedent.Set(antecedent);
            FailedOutcomeValue = failedOutcomeValue;
        }
        
        internal RepeatableAndThenContinuation(BasicCompletes parent, RepeatableCompletes<TAntecedentResult> antecedent, Delegate function) : this(parent, antecedent, Optional.Empty<TResult>(), function)
        {
        }

        internal override bool InnerInvoke(BasicCompletes completedCompletes)
        {
            if (HasFailedValue.Get())
            {
                return false;
            }
            
            base.InnerInvoke(completedCompletes);

            if (Action is Func<TAntecedentResult, ICompletes<TResult>> funcCompletes)
            {
                funcCompletes(_antecedent.Get()!.Outcome).AndThenConsume(t =>
                {
                    OutcomeValue.Set(t);
                    TransformedResult = t;
                });
                return true;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                OutcomeValue.Set(function(_antecedent.Get()!.Outcome));
                TransformedResult = OutcomeValue.Get();
                return true;
            }

            return false;
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
                if (completes.HasOutcome)
                {
                    HasFailedValue.Set(HasFailedValue.Get() || completes.Outcome!.Equals(FailedOutcomeValue.Get()));  
                }
            }
        }
    }
}