// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Operations
{
    public class AndThen<TReceives, TExposes> : Operation<TReceives, TExposes>
    {
        private Func<TReceives, TExposes> mapper;

        public AndThen(Func<TReceives, TExposes> mapper) => this.mapper = mapper;

        public override void OnOutcome(TReceives receives)
        {
            try
            {
                var outcome = mapper(receives);
                EmitOutcome(outcome);
            }
            catch (Exception ex)
            {
                EmitError(ex);
            }
        }
    }
}