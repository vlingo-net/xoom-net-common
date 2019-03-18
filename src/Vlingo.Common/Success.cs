// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public class Success<TCause, TValue> : IOutcome<TCause, TValue> where TCause : Exception
    {
        private readonly TValue value;

        internal Success(TValue value)
        {
            this.value = value;
        }

        public virtual IOutcome<TCause, Tuple<TValue, TSecondSuccess>> AlongWith<TOtherFailure, TSecondSuccess>(IOutcome<TOtherFailure, TSecondSuccess> outcome) where TOtherFailure : Exception
        {
            return outcome.AndThenTo(
                secondOutcome => Success.Of<TCause, Tuple<TValue, TSecondSuccess>>(Tuple.Create(value, secondOutcome)));
        }

        public virtual IOutcome<TCause, TNextSuccess> AndThen<TNextSuccess>(Func<TValue, TNextSuccess> action)
        {
            return Success.Of<TCause, TNextSuccess>(action.Invoke(value));
        }

        public virtual IOutcome<TNextFailure, TNextSuccess> AndThenTo<TNextFailure, TNextSuccess>(Func<TValue, IOutcome<TNextFailure, TNextSuccess>> action) where TNextFailure : Exception
        {
            return action.Invoke(value);
        }

        public virtual ICompletes<TValue> AsCompletes()
        {
            return CompletesExt.WithSuccess(value);
        }

        public virtual Optional<TValue> AsOptional()
        {
            return Optional.Of(value);
        }

        public virtual void AtLeastConsume(Action<TValue> consumer)
        {
            consumer.Invoke(value);
        }

        public virtual IOutcome<NoSuchElementException, TValue> Filter(Func<TValue, bool> filterFunction)
        {
            if (filterFunction.Invoke(value))
            {
                return Success.Of<NoSuchElementException, TValue>(value);
            }

            return Failure.Of<NoSuchElementException, TValue>(new NoSuchElementException($"{value}, null", null));
        }

        public virtual TValue Get()
        {
            return value;
        }

        public virtual TValue GetOrNull()
        {
            return value;
        }

        public virtual IOutcome<TCause, TValue> Otherwise(Func<TCause, TValue> action)
        {
            return this;
        }

        public virtual IOutcome<TNextFailure, TNextSuccess> OtherwiseTo<TNextFailure, TNextSuccess>(Func<TCause, IOutcome<TNextFailure, TNextSuccess>> action) where TNextFailure : Exception
        {
            return Success.Of<TNextFailure, TNextSuccess>((TNextSuccess)(object)value);
        }

        public virtual TNextSuccess Resolve<TNextSuccess>(Func<TCause, TNextSuccess> onFailedOutcome, Func<TValue, TNextSuccess> onSuccessfulOutcome)
        {
            return onSuccessfulOutcome.Invoke(value);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Success<TCause, TValue>;

            if (other == null)
            {
                return false;
            }

            return Equals(value, other.value);
        }

        public override int GetHashCode()
        {
            return 31 * value.GetHashCode();
        }
    }

    public static class Success
    {
        public static IOutcome<TCause, TValue> Of<TCause, TValue>(TValue value) where TCause : Exception
        {
            return new Success<TCause, TValue>(value);
        }
    }
}
