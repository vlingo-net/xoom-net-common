// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Completes.Barrier;

namespace Vlingo.Common.Completes
{
    public class AndThenConsume<TInput, TNextOutput> : IOperation<TInput, TInput, TNextOutput>
    {
        private readonly TimeBarrier timeBarrier;
        private readonly Action<TInput> consumer;
        private IOperation<TInput> nextOperation;

        public AndThenConsume(Scheduler scheduler, TimeSpan timeout, Action<TInput> consumer)
        {
            timeBarrier = new TimeBarrier(scheduler, timeout);
            this.consumer = consumer;
        }

        public void AddSubscriber<TLastOutput>(IOperation<TInput, TNextOutput, TLastOutput> operation)
            => nextOperation = operation;

        public void OnError(Exception ex) => nextOperation.OnError(ex);

        public void OnFailure(TInput outcome) => nextOperation.OnFailure(outcome);

        public void OnOutcome(TInput outcome)
        {
            timeBarrier.Initialize();
            timeBarrier.Execute(new AndThenConsumeRunnable(consumer, outcome, nextOperation), nextOperation);
        }

        private class AndThenConsumeRunnable : IRunnable
        {
            private readonly Action<TInput> consumer;
            private readonly TInput outcome;
            private readonly IOperation<TInput> nextOperation;

            public AndThenConsumeRunnable(Action<TInput> consumer, TInput outcome, IOperation<TInput> nextOperation)
            {
                this.consumer = consumer;
                this.outcome = outcome;
                this.nextOperation = nextOperation;
            }

            public void Run()
            {
                try
                {
                    consumer.Invoke(outcome);
                    nextOperation.OnOutcome(outcome);
                }
                catch(Exception ex)
                {
                    nextOperation.OnError(ex);
                }
            }
        }
    }
}
