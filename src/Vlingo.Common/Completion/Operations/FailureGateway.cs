// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common.Completion.Exceptions;

namespace Vlingo.Common.Completion.Operations
{
    public class FailureGateway<T> : Operation<T, T>
    {
        private T _failureOutcome;

        public FailureGateway(T failureOutcome) => _failureOutcome = failureOutcome;

        public override void OnOutcome(T receives)
        {
            if (receives.Equals(_failureOutcome))
            {
                EmitError(new FailedOperationException(receives));
            }
            else
            {
                EmitOutcome(receives);
            }
        }
    }
}