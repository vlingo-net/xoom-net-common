// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Common.Completion.Continuations;

namespace Vlingo.Common.Completion
{
    public abstract class BasicCompletes
    {
        protected readonly Delegate Action;    // The body of the function. Might be Action<object>, Action<TState> or Action.  Or possibly a Func.
        protected BasicCompletes Parent;
        protected readonly AtomicBoolean ReadyToExectue = new AtomicBoolean(false);
        protected readonly AtomicBoolean Accessible = new AtomicBoolean(false);
        protected readonly AtomicReference<Exception> ExceptionValue = new AtomicReference<Exception>();
        internal readonly AtomicBoolean HasFailedValue = new AtomicBoolean(false);
        internal readonly Scheduler Scheduler;
        internal readonly ConcurrentQueue<CompletesContinuation> Continuations = new ConcurrentQueue<CompletesContinuation>();
        internal CompletesContinuation FailureContinuation;
        internal CompletesContinuation ExceptionContinuation;
        //internal object CompletesResult;
        internal object TransformedResult;

        protected BasicCompletes(Delegate action) : this(null, action)
        {
        }

        protected BasicCompletes(Scheduler scheduler, Delegate action)
        {
            Scheduler = scheduler;
            Action = action;
        }

        internal abstract void InnerInvoke(BasicCompletes completedCompletes);
        
        internal abstract void UpdateFailure(BasicCompletes previousContinuation);

        internal abstract void RunContinuationsWhenReady();

        internal Exception Exception => ExceptionValue.Get();

        private void RegisterContinuation(CompletesContinuation continuationCompletes) =>
            Continuations.Enqueue(continuationCompletes);
        
        private void RegisterFailureContinuation(CompletesContinuation continuationCompletes) =>
            FailureContinuation = continuationCompletes;

        private void RegisterExceptionContinuation(CompletesContinuation continuationCompletes) =>
            ExceptionContinuation = continuationCompletes;

        internal void AndThenInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
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
        }

        internal void RecoverInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            RegisterExceptionContinuation(continuation);
        }
    }
}