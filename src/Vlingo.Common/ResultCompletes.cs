// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public class ResultCompletes : ICompletes<object>
    {
        internal ICompletes<object> __internal__clientCompletes;
        internal object __internal__outcome = null;
        internal bool __internal__outcomeSet = false;

        public virtual bool IsCompleted => throw new NotSupportedException();

        public virtual bool HasFailed => throw new NotSupportedException();

        public virtual bool HasOutcome => throw new NotSupportedException();

        public virtual object Outcome => throw new NotSupportedException();

        public virtual ICompletes<object> AndThen(long timeout, object failedOutcomeValue, Func<object, object> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> AndThen(object failedOutcomeValue, Func<object, object> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> AndThen(long timeout, Func<object, object> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> AndThen(Func<object, object> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> AndThenConsume(long timeout, object failedOutcomeValue, Action<object> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> AndThenConsume(object failedOutcomeValue, Action<object> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> AndThenConsume(long timeout, Action<object> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> AndThenConsume(Action<object> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<F, O>(long timeout, F failedOutcomeValue, Func<object, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<F, O>(F failedOutcomeValue, Func<object, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<O>(long timeout, Func<object, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<O>(Func<object, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual object Await()
        {
            throw new NotSupportedException();
        }

        public virtual object Await(long timeout)
        {
            throw new NotSupportedException();
        }

        public virtual void Failed()
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> Otherwise(Func<object, object> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> OtherwiseConsume(Action<object> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> RecoverFrom(Func<Exception, object> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<object> Repeat()
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<O> With<O>(O outcome)
        {
            this.__internal__outcomeSet = true;
            this.__internal__outcome = outcome;
            return (ICompletes<O>)this;
        }

        public virtual ICompletes<object> ClientCompletes()
        {
            return __internal__clientCompletes;
        }

        public virtual void Reset(ICompletes<object> clientCompletes)
        {
            this.__internal__clientCompletes = clientCompletes;
            this.__internal__outcome = null;
            this.__internal__outcomeSet = false;
        }
    }
}
