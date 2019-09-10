// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
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
        public RepeatableCompletes(Scheduler scheduler) : base(new RepeatableActiveState(scheduler))
        {
        }

        public RepeatableCompletes(T outcome, bool succeeded) : base(new RepeatableActiveState(), outcome, succeeded)
        {
        }

        public RepeatableCompletes(T outcome) : base(new RepeatableActiveState(), outcome)
        {
        }

        public override ICompletes<T> Repeat()
        {
            if (state.IsOutcomeKnown)
            {
                state.Repeat();
            }
            return this;
        }

        public override ICompletes<TO> With<TO>(TO outcome)
        {
            base.With(outcome);
            state.Repeat();
            return (ICompletes<TO>)this;
        }

        protected internal class RepeatableActiveState : BasicActiveState
        {
            private readonly ConcurrentQueue<Action<T>> actionsBackup;
            private readonly AtomicBoolean repeating;

            protected internal RepeatableActiveState(Scheduler scheduler) : base(scheduler)
            {
                actionsBackup = new ConcurrentQueue<Action<T>>();
                repeating = new AtomicBoolean(false);
            }

            protected internal RepeatableActiveState() : this(null)
            {
            }

            public override void BackUp(Action<T> action)
            {
                if(action != null)
                {
                    actionsBackup.Enqueue(action);
                }
            }

            public override void Repeat()
            {
                if (repeating.CompareAndSet(false, true))
                {
                    Restore();
                    IsOutcomeKnown = false;
                    repeating.Set(false);
                }
            }

            public override void Restore()
            {
                while (!actionsBackup.IsEmpty)
                {
                    if (actionsBackup.TryDequeue(out var action))
                    {
                        Restore(action);
                    }
                }
            }
        }
    }
}
