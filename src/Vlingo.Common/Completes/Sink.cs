// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace Vlingo.Common.Completes
{
    public class Sink<TInput, TOutput> : IOperation<TInput, TInput, TOutput>
    {
        private bool completed;
        private CountdownEvent latch = new CountdownEvent(1);

        public Exception Error { get; private set; }

        public TInput Outcome { get; private set; }

        public bool HasOutcome { get; private set; }

        public bool HasFailed { get; private set; }

        public bool HasErrored { get; private set; }

        public bool IsCompleted => HasOutcome || HasFailed || HasErrored;

        public void PipeIfNeeded<TNextOutput>(IOperation<TInput, TOutput, TNextOutput> op)
        {
            if (HasErrored)
            {
                op.OnError(Error);
                ResetLatch();
            }
            else if (HasFailed)
            {
                op.OnFailure(Outcome);
                ResetLatch();
            }
            else if (completed)
            {
                op.OnOutcome(Outcome);
                ResetLatch();
            }
        }

        public void Repeat()
        {
            if (IsCompleted)
            {
                Outcome = default(TInput);
                HasOutcome = false;
                HasFailed = false;
                HasErrored = false;
                Error = null;
                ResetLatch();
            }
        }

        public void AddSubscriber<TLastOutput>(Completes.IOperation<TInput, TOutput, TLastOutput> operation)
            => throw new InvalidOperationException("You can't subscribe to a sink.");

        public void OnError(Exception ex)
        {
            Error = ex;
            HasErrored = true;
            completed = true;
            latch.Signal();
        }

        public void OnFailure(TInput outcome)
        {
            Outcome = outcome;
            HasFailed = true;
            completed = true;
            latch.Signal();
        }

        public void OnOutcome(TInput outcome)
        {
            Outcome = outcome;
            HasOutcome = true;
            completed = true;
            latch.Signal();
        }

        public TInput Await(TimeSpan timeout)
        {
            try
            {
                latch.Wait(timeout);
            }
            catch
            {
            }

            return Outcome;
        }

        private void ResetLatch() => latch = new CountdownEvent(1);
    }
}
