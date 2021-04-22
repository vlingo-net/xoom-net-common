// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Common.Completion;
using Vlingo.Xoom.Common.Completion.Continuations;

namespace Vlingo.Xoom.Common
{
    public class RepeatableCompletes<TResult> : BasicCompletes<TResult>
    {
        private readonly ConcurrentQueue<CompletesContinuation> _continuationsBackup = new ConcurrentQueue<CompletesContinuation>();
        private readonly AtomicBoolean _repeating = new AtomicBoolean(false);

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

        public override ICompletes<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var parent = Parent ?? this;
            var continuationCompletes = new RepeatableAndThenContinuation<TResult, TNewResult>(parent, this, Optional.Of(failedOutcomeValue), function);
            parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }
        
        public override ICompletes<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var parent = Parent ?? this;
            var continuationCompletes = new RepeatableAndThenContinuation<TResult, TNewResult>(parent, this, function);
            parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }
        
        public override ICompletes<TResult> AndThen(Func<TResult, TResult> function)
        {
            var parent = Parent ?? this;
            var continuationCompletes = new RepeatableAndThenContinuation<TResult, TResult>(parent, this, function);
            parent.AndThenInternal(continuationCompletes);
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
            HasException.Set(false);
            HasFailedValue.Set(false);
            base.With(outcome);
            ReadyToExectue.Set(false);
            RepeatInternal();
            return this;   
        }

        internal override void BackUp(CompletesContinuation? continuation)
        {
            if (continuation != null)
            {
                _continuationsBackup.Enqueue(continuation);
            }
        }

        protected override void Restore()
        {
            while (!_continuationsBackup.IsEmpty)
            {
                if (_continuationsBackup.TryDequeue(out var continuation))
                {
                    Restore(continuation);
                }
            }
        }

        private void RepeatInternal()
        {
            if (_repeating.CompareAndSet(false, true))
            {
                Restore();
                _repeating.Set(false);
            }
        }
    }
}