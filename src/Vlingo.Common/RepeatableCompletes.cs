// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Concurrent;

namespace Vlingo.Common
{
    public class RepeatableCompletes<T> : BasicCompletes<T>
    {
        public RepeatableCompletes(Scheduler scheduler) : base(new RepeatableActiveState<T>(scheduler))
        {
        }

        public RepeatableCompletes(T outcome, bool succeeded) : base(new RepeatableActiveState<T>(), outcome, succeeded)
        {
        }

        public RepeatableCompletes(T outcome) : base(new RepeatableActiveState<T>(), outcome)
        {
        }

        public override ICompletes<T> Repeat()
        {
            if (state.IsCompleted)
            {
                state.Repeat();
            }
            return this;
        }

        public override ICompletes<O> With<O>(O outcome)
        {
            state.Outcome((T)(object)outcome);
            state.Repeat();
            return (ICompletes<O>)this;
        }

        protected internal class RepeatableActiveState<TRActSt> : BasicActiveState<TRActSt>
        {
            private readonly ConcurrentQueue<Action<TRActSt>> actionsBackup;
            private readonly ConcurrentQueue<TRActSt> pendingOutcomes;
            private readonly AtomicBoolean repeating;

            protected internal RepeatableActiveState(Scheduler scheduler) : base(scheduler)
            {
                actionsBackup = new ConcurrentQueue<Action<TRActSt>>();
                pendingOutcomes = new ConcurrentQueue<TRActSt>();
                repeating = new AtomicBoolean(false);
            }

            protected internal RepeatableActiveState() : this(null)
            {
            }

            public override Action<TRActSt> Action()
            {
                var action = base.Action();
                BackUp(action);
                return action;
            }

            public override void Outcome(TRActSt outcome)
            {
                CancelTimer();
                pendingOutcomes.Enqueue(outcome);
            }

            public override void Repeat()
            {
                if (repeating.CompareAndSet(false, true))
                {
                    while (pendingOutcomes.TryDequeue(out var pendingOutcome))
                    {
                        CompletedWith(pendingOutcome);
                        Restore();
                    }
                    repeating.Set(false);
                }
            }

            private void BackUp(Action<TRActSt> action)
            {
                if (action != null)
                {
                    actionsBackup.Enqueue(action);
                }
            }

            private void Restore()
            {
                while (actionsBackup.TryDequeue(out var action))
                {
                    Action(action);
                }
            }
        }
    }
}
