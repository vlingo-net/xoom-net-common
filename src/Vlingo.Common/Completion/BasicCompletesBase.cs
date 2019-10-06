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
        protected readonly AtomicBoolean ReadyToExectue = new AtomicBoolean(false);
        protected readonly AtomicBoolean Accessible = new AtomicBoolean(false);
        internal readonly Scheduler Scheduler;
        internal readonly ConcurrentQueue<CompletesContinuation> Continuations = new ConcurrentQueue<CompletesContinuation>();
        internal CompletesContinuation FailureContinuation;
        internal CompletesContinuation ExceptionContinuation;
        internal object CompletesResult;
        internal object TransformedResult;

        protected BasicCompletes(Delegate action) => Action = action;

        protected BasicCompletes(Scheduler scheduler, Delegate action)
        {
            Scheduler = scheduler;
            Action = action;
        }

        public virtual bool HasFailed { get; } = false;

        internal virtual BasicCompletes Antecedent { get; } = null;
        
        internal abstract void InnerInvoke(BasicCompletes completedCompletes);
        
        internal abstract void UpdateFailure(BasicCompletes previousContinuation);

        internal abstract void HandleFailure();

        internal abstract void HandleException(Exception e);
        
        internal abstract Exception Exception { get; }
        
        internal virtual void RegisterContinuation(CompletesContinuation continuation) => Continuations.Enqueue(continuation);

        internal virtual void RegisterFailureContiuation(CompletesContinuation continuationCompletes) =>
            FailureContinuation = continuationCompletes;

        internal virtual void RegisterExceptionContiuation(CompletesContinuation continuationCompletes) =>
            ExceptionContinuation = continuationCompletes;

        protected void AndThenInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterContinuation(continuation);
            if (ReadyToExectue.Get())
            {
                continuation.Run(continuationCompletes.Antecedent);
            }
        }

        protected void OtherwiseInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterFailureContiuation(continuation);
        }

        protected void RecoverInternal(BasicCompletes continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterExceptionContiuation(continuation);
        }
    }
}