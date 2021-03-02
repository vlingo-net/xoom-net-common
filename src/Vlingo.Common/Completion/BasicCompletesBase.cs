// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;
using Vlingo.Common.Completion.Continuations;

namespace Vlingo.Common.Completion
{
    public abstract class BasicCompletes
    {
        private readonly int _id;
        protected readonly Delegate Action;    // The body of the function. Might be Action<object>, Action<TState> or Action.  Or possibly a Func.
        protected readonly BasicCompletes? Parent;
        protected readonly AtomicBoolean ReadyToExectue = new AtomicBoolean(false);
        internal readonly AtomicReference<Exception> ExceptionValue = new AtomicReference<Exception>();
        internal readonly AtomicBoolean TimedOut = new AtomicBoolean(false);
        internal readonly ManualResetEventSlim OutcomeKnown = new ManualResetEventSlim(false);
        internal readonly AtomicBoolean HasFailedValue = new AtomicBoolean(false);
        internal readonly AtomicBoolean HasException = new AtomicBoolean(false);
        internal readonly Scheduler? Scheduler;
        internal readonly ConcurrentQueue<CompletesContinuation> Continuations = new ConcurrentQueue<CompletesContinuation>();
        internal CompletesContinuation? FailureContinuation;
        internal CompletesContinuation? ExceptionContinuation;
        internal object? TransformedResult;

        protected BasicCompletes(Delegate action, BasicCompletes? parent) : this(null, action, parent)
        {
        }

        protected BasicCompletes(Scheduler? scheduler, Delegate action, BasicCompletes? parent)
        {
            _id = new Random().Next(1, 1000);
            Parent = parent;
            Scheduler = scheduler;
            Action = action;
        }
        
        internal abstract bool InnerInvoke(BasicCompletes completedCompletes);
        
        internal abstract void UpdateFailure(BasicCompletes previousContinuation);

        internal abstract void HandleException(Exception e);

        internal abstract void BackUp(CompletesContinuation continuation);
        
        internal abstract bool CanBeExecuted();
        
        internal void OnResultAvailable(BasicCompletes lastRunContinuation) => RunContinuationsWhenReady(lastRunContinuation);

        internal Exception? Exception => ExceptionValue.Get();

        internal void AndThenInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            AndThenInternal(continuation);
        }
        
        internal void AndThenInternal(CompletesContinuation continuation)
        {
            RegisterContinuation(continuation);
            if (ReadyToExectue.Get())
            {
                RunContinuationsWhenReady();
            }
        }

        internal void OtherwiseInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            RegisterFailureContinuation(continuation);
            if (OutcomeKnown.IsSet && HasFailedValue.Get())
            {
                continuation.Run(this);
                RunContinuationsWhenReady(continuationCompletes);
            }
        }

        internal void RecoverInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            RegisterExceptionContinuation(continuation);
        }

        protected abstract void RunContinuationsWhenReady();

        protected abstract void RunContinuationsWhenReady(BasicCompletes completedContinuation);

        public override string ToString() => $"BasicCompletes [{_id}]";
        
        private void RegisterContinuation(CompletesContinuation continuationCompletes)
        {
            if (HasException.Get())
            {
                var currentContinuations = new CompletesContinuation[Continuations.Count];
                Continuations.CopyTo(currentContinuations, 0);
                var continuationsCount = Continuations.Count;
                for (var i = 0; i < continuationsCount; i++)
                {
                    Continuations.TryDequeue(out _);
                }
                Continuations.Enqueue(continuationCompletes);
                foreach (var currentContinuation in currentContinuations)
                {
                    Continuations.Enqueue(currentContinuation);
                }
            }
            else
            {
                Continuations.Enqueue(continuationCompletes);
            }
        }

        private void RegisterFailureContinuation(CompletesContinuation continuationCompletes) =>
            FailureContinuation = continuationCompletes;

        private void RegisterExceptionContinuation(CompletesContinuation continuationCompletes) =>
            ExceptionContinuation = continuationCompletes;
    }
}