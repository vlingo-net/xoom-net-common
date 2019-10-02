// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public class Failure<TCause, TValue> : IOutcome<TCause, TValue> where TCause : Exception
    {
        private readonly TCause cause;

        internal Failure(TCause cause)
        {
            this.cause = cause;
        }
        
        public virtual IOutcome<TCause, Tuple<TValue, TSecondSuccess>> AlongWith<TOtherFailure, TSecondSuccess>(IOutcome<TOtherFailure, TSecondSuccess> outcome) where TOtherFailure : Exception
        {
            return Failure.Of<TCause, Tuple<TValue, TSecondSuccess>>(cause);
        }

        public virtual IOutcome<TCause, TNextSuccess> AndThen<TNextSuccess>(Func<TValue, TNextSuccess> action)
        {
            return Failure.Of<TCause, TNextSuccess>(cause);
        }

        public virtual IOutcome<TNextFailure, TNextSuccess> AndThenTo<TNextFailure, TNextSuccess>(Func<TValue, IOutcome<TNextFailure, TNextSuccess>> action) where TNextFailure : Exception
        {
            return Failure.Of<TNextFailure, TNextSuccess>((TNextFailure)(Exception)cause);
        }

        public virtual ICompletes<TValue> AsCompletes()
        {
            return Completes2.WithFailure<TValue>();
        }

        public virtual Optional<TValue> AsOptional()
        {
            return Optional.Empty<TValue>();
        }

        public virtual void AtLeastConsume(Action<TValue> consumer)
        {
        }

        public virtual IOutcome<NoSuchElementException, TValue> Filter(Func<TValue, bool> filterFunction)
        {
            return Failure.Of<NoSuchElementException, TValue>(new NoSuchElementException(cause));
        }

        public virtual TValue Get()
        {
            throw cause;
        }

        public virtual TValue GetOrNull()
        {
            return default(TValue);
        }

        public virtual IOutcome<TCause, TValue> Otherwise(Func<TCause, TValue> action)
        {
            return Success.Of<TCause, TValue>(action.Invoke(cause));
        }

        public virtual IOutcome<TNextFailure, TNextSuccess> OtherwiseTo<TNextFailure, TNextSuccess>(Func<TCause, IOutcome<TNextFailure, TNextSuccess>> action) where TNextFailure : Exception
        {
            return action.Invoke(cause);
        }

        public virtual TNextSuccess Resolve<TNextSuccess>(Func<TCause, TNextSuccess> onFailedOutcome, Func<TValue, TNextSuccess> onSuccessfulOutcome)
        {
            return onFailedOutcome.Invoke(cause);
        }

        public virtual IOutcome<TNextFailure, TValue> OtherwiseFail<TNextFailure>(Func<TCause, TNextFailure> action) 
            where TNextFailure : Exception
            => Failure.Of<TNextFailure, TValue>(action.Invoke(cause));

        public override bool Equals(object obj)
        {
            var other = obj as Failure<TCause, TValue>;

            if (other == null)
            {
                return false;
            }

            return Equals(cause, other.cause);
        }

        public override int GetHashCode()
        {
            return 31 * cause.GetHashCode();
        }
    }

    public static class Failure
    {
        public static IOutcome<TCause, TValue> Of<TCause, TValue>(TCause cause) where TCause : Exception
        {
            return new Failure<TCause, TValue>(cause);
        }
    }
}
