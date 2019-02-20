// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completes
{
    public class OtherwiseConsume<TInput, TNextOutput> : IOperation<TInput, TInput, TNextOutput>
    {
        private readonly Action<TInput> consumer;
        private IOperation<TInput> nextOperation;

        public OtherwiseConsume(Action<TInput> consumer)
        {
            this.consumer = consumer;
        }

        public void AddSubscriber<TLastOutput>(IOperation<TInput, TNextOutput, TLastOutput> operation)
            => nextOperation = operation;

        public void OnError(Exception ex) => nextOperation.OnError(ex);

        public void OnFailure(TInput outcome)
        {
            try
            {
                consumer.Invoke(outcome);
                nextOperation.OnFailure(outcome);
            }
            catch(Exception ex)
            {
                nextOperation.OnError(ex);
            }
        }

        public void OnOutcome(TInput outcome) => nextOperation.OnOutcome(outcome);
    }
}
