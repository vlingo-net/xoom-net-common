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
    public class AndThenTo<TInput, TOutput, TNextOutput> : IOperation<TInput, TOutput, TNextOutput>
    {
        private readonly TimeBarrier timeBarrier;
        private readonly Func<TInput, ICompletes<TOutput>> mapper;
        private readonly TOutput failedOutcome;

        private IOperation<TOutput> nextOperation;

        public AndThenTo(Scheduler scheduler, TimeSpan timeout, Func<TInput, ICompletes<TOutput>> mapper, TOutput failedOutcome)
        {
            timeBarrier = new TimeBarrier(scheduler, timeout);
            this.mapper = mapper;
            this.failedOutcome = failedOutcome;
        }

        public void AddSubscriber<TLastOutput>(IOperation<TOutput, TNextOutput, TLastOutput> operation)
            => nextOperation = operation;

        public void OnError(Exception ex) => nextOperation.OnError(ex);

        public void OnFailure(TInput outcome) => nextOperation.OnFailure((TOutput)(object)outcome);

        public void OnOutcome(TInput outcome)
        {
            var completes = mapper.Invoke(outcome);
            timeBarrier.Initialize();
            completes
                .AndThenConsume(v => timeBarrier.Execute(new AndThenToSuccessRunnable(v, failedOutcome, nextOperation), nextOperation))
                .OtherwiseConsume(v => timeBarrier.Execute(new AndThenToFailureRunnable(v, nextOperation), nextOperation));
        }

        private class AndThenToSuccessRunnable : IRunnable
        {
            private readonly TOutput v;
            private readonly TOutput failedOutcome;
            private readonly IOperation<TOutput> nextOperation;

            public AndThenToSuccessRunnable(TOutput v, TOutput failedOutcome, IOperation<TOutput> nextOperation)
            {
                this.v = v;
                this.failedOutcome = failedOutcome;
                this.nextOperation = nextOperation;
            }

            public void Run()
            {
                if(Equals(v, failedOutcome))
                {
                    nextOperation.OnFailure(v);
                }
                else
                {
                    nextOperation.OnOutcome(v);
                }
            }
        }

        private class AndThenToFailureRunnable : IRunnable
        {
            private readonly TOutput v;
            private readonly IOperation<TOutput> nextOperation;

            public AndThenToFailureRunnable(TOutput v, IOperation<TOutput> nextOperation)
            {
                this.v = v;
                this.nextOperation = nextOperation;
            }
            public void Run() => nextOperation.OnFailure(v);
        }
    }
}
