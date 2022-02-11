// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Common;

public class Success<TCause, TValue> : IOutcome<TCause, TValue> where TCause : Exception
{
    private readonly TValue _value;

    internal Success(TValue value) => _value = value;

    public virtual IOutcome<TCause, Tuple<TValue, TSecondSuccess>> AlongWith<TOtherFailure, TSecondSuccess>(
        IOutcome<TOtherFailure, TSecondSuccess> outcome) where TOtherFailure : Exception =>
        outcome.AndThenTo(
            secondOutcome => Success.Of<TCause, Tuple<TValue, TSecondSuccess>>(Tuple.Create(_value, secondOutcome)));

    public virtual IOutcome<TCause, TNextSuccess> AndThen<TNextSuccess>(Func<TValue, TNextSuccess> action) =>
        Success.Of<TCause, TNextSuccess>(action.Invoke(_value));

    public virtual IOutcome<TNextFailure, TNextSuccess> AndThenTo<TNextFailure, TNextSuccess>(
        Func<TValue, IOutcome<TNextFailure, TNextSuccess>> action) where TNextFailure : Exception =>
        action.Invoke(_value);

    public virtual ICompletes<TValue> AsCompletes() => Completes.WithSuccess(_value);

    public virtual Optional<TValue> AsOptional() => Optional.Of(_value);

    public virtual void AtLeastConsume(Action<TValue> consumer) => consumer.Invoke(_value);

    public virtual IOutcome<NoSuchElementException, TValue> Filter(Func<TValue, bool> filterFunction)
    {
        if (filterFunction.Invoke(_value))
        {
            return Success.Of<NoSuchElementException, TValue>(_value);
        }

        return Failure.Of<NoSuchElementException, TValue>(new NoSuchElementException($"{_value}, null", null));
    }

    public virtual TValue Get() => _value;

    public virtual TValue GetOrNull() => _value;

    public virtual IOutcome<TCause, TValue> Otherwise(Func<TCause, TValue> action) => this;

    public virtual IOutcome<TNextFailure, TNextSuccess> OtherwiseTo<TNextFailure, TNextSuccess>(
        Func<TCause, IOutcome<TNextFailure, TNextSuccess>> action) where TNextFailure : Exception =>
        Success.Of<TNextFailure, TNextSuccess>((TNextSuccess)(object)_value!);

    public virtual TNextSuccess Resolve<TNextSuccess>(Func<TCause, TNextSuccess> onFailedOutcome,
        Func<TValue, TNextSuccess> onSuccessfulOutcome) => onSuccessfulOutcome.Invoke(_value);

    public virtual IOutcome<TNextFailure, TValue> OtherwiseFail<TNextFailure>(Func<TCause, TNextFailure> action)
        where TNextFailure : Exception
        => Success.Of<TNextFailure, TValue>(_value);

    public override bool Equals(object? obj)
    {
        var other = obj as Success<TCause, TValue>;

        if (other == null)
        {
            return false;
        }

        return Equals(_value, other._value);
    }

    public override int GetHashCode() => 31 * _value!.GetHashCode();
}

public static class Success
{
    public static IOutcome<TCause, TValue> Of<TCause, TValue>(TValue value) where TCause : Exception =>
        new Success<TCause, TValue>(value);
}