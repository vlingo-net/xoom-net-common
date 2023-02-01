// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Common;

public sealed class Optional<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    internal Optional(T value)
    {
        _value = value;
        _hasValue = true;
    }

    internal Optional()
    {
        _value = default!;
        _hasValue = false;
    }

    public Optional<T> Filter(Func<T, bool> predicate)
    {
        if (_hasValue && predicate(_value))
        {
            return this;
        }

        return Optional.Empty<T>();
    }

    public Optional<U> FlatMap<U>(Func<T, Optional<U>> mapper)
    {
        if (!_hasValue)
        {
            return Optional.Empty<U>();
        }

        return mapper(_value);
    }

    public T Get() => _value;

    public void IfPresent(Action<T> consumer)
    {
        if (_hasValue)
        {
            consumer(_value);
        }
    }

    public bool IsPresent => _hasValue;

    public Optional<U> Map<U>(Func<T, U> mapper)
    {
        if (!_hasValue)
        {
            return Optional.Empty<U>();
        }

        return Optional.Of(mapper(_value));
    }

    public T OrElse(T other)
    {
        return _hasValue ?
            _value :
            other;
    }

    public T OrElseGet(Func<T> supplier)
    {
        return _hasValue ?
            _value :
            supplier();
    }

    public T OrElseThrow<X>(Func<X> supplier) where X : Exception
    {
        if (_hasValue)
        {
            return _value;
        }

        supplier();
        return default!;
    }

    public override bool Equals(object? obj)
    {
        var other = obj as Optional<T>;
        if (other == null)
        {
            return false;
        }

        if(!other._hasValue && !_hasValue)
        {
            return true; // both empty
        }

        if(!other._hasValue || !_hasValue || !Equals(_value, other._value))
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override string ToString()
    {
        if (!_hasValue)
        {
            return "Empty()";
        }

        return $"Value<{typeof(T).FullName}>[{_value!.ToString()}]";
    }
}

public static class Optional
{
    public static Optional<T> Empty<T>() => new Optional<T>();

    public static Optional<T> Of<T>(T value) => new Optional<T>(value);

    public static Optional<TNullable> OfNullable<TNullable>(TNullable? value) where TNullable : class
    {
        if (value == null)
        {
            return Empty<TNullable>();
        }

        return new Optional<TNullable>(value);
    }
}