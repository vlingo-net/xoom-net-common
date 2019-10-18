// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Common.Completion;
using Vlingo.Common.Completion.Continuations;

namespace Vlingo.Common
{
    public class RepeatableCompletes<TResult> : BasicCompletes<TResult>
    {
        private readonly ConcurrentQueue<CompletesContinuation> continuationsBackup = new ConcurrentQueue<CompletesContinuation>();
        private readonly AtomicBoolean repeating = new AtomicBoolean(false);

        public RepeatableCompletes(Scheduler scheduler) : this(scheduler, null)
        {
        }
        
        public RepeatableCompletes(Scheduler scheduler, BasicCompletes? parent) : base(scheduler, parent)
        {
        }
        
        public RepeatableCompletes(TResult outcome, bool succeeded) : this(outcome, succeeded, null)
        {
        }

        public RepeatableCompletes(TResult outcome, bool succeeded, BasicCompletes? parent) : base(outcome, succeeded, parent)
        {
        }

        public RepeatableCompletes(TResult outcome) : base(outcome)
        {
        }

        protected RepeatableCompletes(Delegate valueSelector, BasicCompletes? parent) : base(valueSelector, parent)
        {
        }
        
        public override ICompletes<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new RepeatableAndThenContinuation<TResult, TNewResult>(Parent, this, function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public override ICompletes<TResult> Repeat()
        {
            if (OutcomeKnown.IsSet)
            {
                RepeatInternal();
            }
            
            return this;
        }

        public override ICompletes<TResult> With(TResult outcome)
        {
            base.With(outcome);
            ReadyToExectue.Set(false);
            RepeatInternal();
            return this;   
        }

        internal override void BackUp(CompletesContinuation continuation)
        {
            if (continuation != null)
            {
                continuationsBackup.Enqueue(continuation);
            }
        }

        protected override void Restore()
        {
            while (!continuationsBackup.IsEmpty)
            {
                if (continuationsBackup.TryDequeue(out var continuation))
                {
                    Restore(continuation);
                }
            }
        }

        private void RepeatInternal()
        {
            if (repeating.CompareAndSet(false, true))
            {
                Restore();
                OutcomeKnown.Reset();
                repeating.Set(false);
            }
        }
    }
}