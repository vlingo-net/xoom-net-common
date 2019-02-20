// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completes
{
    public class Otherwise<TInput, TNextOutput> : IOperation<TInput, TInput, TNextOutput>
    {
        private readonly Func<TInput, TInput> mapper;
        private IOperation<TInput> nextOperation;

        public Otherwise(Func<TInput, TInput> mapper)
        {
            this.mapper = mapper;
        }

        public void AddSubscriber<TLastOutput>(IOperation<TInput, TNextOutput, TLastOutput> operation)
            => nextOperation = operation;

        public void OnError(Exception ex) => nextOperation.OnError(ex);

        public void OnFailure(TInput outcome)
        {
            try
            {
                var next = mapper.Invoke(outcome);
                nextOperation.OnFailure(next);
            }
            catch(Exception ex)
            {
                nextOperation.OnError(ex);
            }
        }

        public void OnOutcome(TInput outcome) => nextOperation.OnOutcome(outcome);
    }
}
