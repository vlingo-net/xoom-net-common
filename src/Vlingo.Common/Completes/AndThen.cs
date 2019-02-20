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
    public class AndThen<TInput, TOutput, TNextOutput> : IOperation<TInput, TOutput, TNextOutput>
    {
        private readonly TimeBarrier timeBarrier;
        private readonly Func<TInput, TOutput> mapper;
        private readonly TOutput failedOutcome;

        private IOperation<TOutput> nextOperation;

        public AndThen(Scheduler scheduler, TimeSpan timeout, Func<TInput, TOutput> mapper, TOutput failedOutcome)
        {
            timeBarrier = new TimeBarrier(scheduler, timeout);
            this.mapper = mapper;
            this.failedOutcome = failedOutcome;
        }

        public static AndThen<TFirst, TFirst, TFirst> Identity<TFirst>(Scheduler scheduler, Sink<TFirst, TFirst> sink)
        {
            var identity = new AndThen<TFirst, TFirst, TFirst>(scheduler, TimeSpan.FromMilliseconds(1000), x => x, default(TFirst));
            identity.AddSubscriber(sink);
            return identity;
        }

        public void AddSubscriber<TLastOutput>(IOperation<TOutput, TNextOutput, TLastOutput> operation)
            => nextOperation = operation;

        public void OnError(Exception ex) => nextOperation.OnError(ex);

        public void OnFailure(TInput outcome) => nextOperation.OnFailure((TOutput)(object)outcome);

        public void OnOutcome(TInput outcome)
        {
            timeBarrier.Initialize();
            timeBarrier.Execute(new AndThenRunnable(mapper, outcome, failedOutcome, nextOperation), nextOperation);
        }

        private class AndThenRunnable : IRunnable
        {
            private readonly Func<TInput, TOutput> mapper;
            private readonly TInput outcome;
            private readonly TOutput failedOutcome;
            private readonly IOperation<TOutput> nextOperation;

            public AndThenRunnable(Func<TInput, TOutput> mapper, TInput outcome, TOutput failedOutcome, IOperation<TOutput> nextOperation)
            {
                this.mapper = mapper;
                this.outcome = outcome;
                this.failedOutcome = failedOutcome;
                this.nextOperation = nextOperation;
            }

            public void Run()
            {
                try
                {
                    var next = mapper.Invoke(outcome);
                    if(Equals(next, failedOutcome))
                    {
                        nextOperation.OnFailure(next);
                    }
                    else
                    {
                        nextOperation.OnOutcome(next);
                    }
                }
                catch(Exception ex)
                {
                    nextOperation.OnError(ex);
                }
            }
        }
    }
}
