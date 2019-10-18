// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public sealed class Optional<T>
    {
        private readonly T value;
        private readonly bool hasValue;

        internal Optional(T value)
        {
            this.value = value;
            hasValue = true;
        }

        internal Optional()
        {
            value = default!;
            hasValue = false;
        }

        public Optional<T> Filter(Func<T, bool> predicate)
        {
            if (hasValue && predicate(value))
            {
                return this;
            }

            return Optional.Empty<T>();
        }

        public Optional<U> FlatMap<U>(Func<T, Optional<U>> mapper)
        {
            if (!hasValue)
            {
                return Optional.Empty<U>();
            }

            return mapper(value);
        }

        public T Get() => value;

        public void IfPresent(Action<T> consumer)
        {
            if (hasValue)
            {
                consumer(value);
            }
        }

        public bool IsPresent => hasValue;

        public Optional<U> Map<U>(Func<T, U> mapper)
        {
            if (!hasValue)
            {
                return Optional.Empty<U>();
            }

            return Optional.Of(mapper(value));
        }

        public T OrElse(T other)
        {
            return hasValue ?
                value :
                other;
        }

        public T OrElseGet(Func<T> supplier)
        {
            return hasValue ?
                value :
                supplier();
        }

        public T OrElseThrow<X>(Func<X> supplier) where X : Exception
        {
            if (hasValue)
            {
                return value;
            }

            supplier();
            return default!;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Optional<T>;
            if (other == null)
            {
                return false;
            }

            if(!other.hasValue && !hasValue)
            {
                return true; // both empty
            }

            if(!other.hasValue || !hasValue || !Equals(value, other.value))
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
            if (!hasValue)
            {
                return "Empty()";
            }

            return $"Value<{typeof(T).FullName}>[{value!.ToString()}]";
        }
    }

    public static class Optional
    {
        public static Optional<T> Empty<T>() => new Optional<T>();

        public static Optional<T> Of<T>(T value) => new Optional<T>(value);

        public static Optional<TNullable> OfNullable<TNullable>(TNullable value) where TNullable : class
        {
            if (value == null)
            {
                return Empty<TNullable>();
            }

            return new Optional<TNullable>(value);
        }
    }
}
