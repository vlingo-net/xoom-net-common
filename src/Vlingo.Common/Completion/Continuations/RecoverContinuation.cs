// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Continuations
{
    internal class RecoverContinuation<TResult> : BasicCompletes<TResult>
    {
        private readonly BasicCompletes<TResult> antecedent;

        internal RecoverContinuation(BasicCompletes<TResult> antecedent, Delegate function) : this(antecedent, function, null)
        {
        }
            
        internal RecoverContinuation(BasicCompletes<TResult> antecedent, Delegate function, BasicCompletes? parent) : base(function, parent) =>
            this.antecedent = antecedent;

        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (Action is Func<Exception, TResult> function)
            {
                if (completedCompletes is BasicCompletes<TResult> basicCompletes)
                {
                    OutcomeValue.Set(function(basicCompletes.Exception!));
                }
            }
        }
    }
}