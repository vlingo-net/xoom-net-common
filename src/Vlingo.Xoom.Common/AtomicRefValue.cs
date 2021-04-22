// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Common
{
    public class AtomicRefValue<T>
    {
        private object? _value;
        private readonly T _defaultValue;

        public AtomicRefValue(object? initialValue)
        {
            _value = initialValue!;
            _defaultValue = default!;
        }

        public AtomicRefValue() : this(default)
        {
        }

        public T Get()
        {
            var result = Interlocked.CompareExchange(ref _value, _defaultValue, _defaultValue);
            if (result == null)
            {
                return _defaultValue;
            }

            return (T) result;
        }

        public T Set(T newValue)
        {
            var result = Interlocked.Exchange(ref _value, newValue);
            if (result == null)
            {
                return _defaultValue;
            }

            return (T) result;
        }

        public T CompareAndSet(T expected, T update)
        {
            var result = Interlocked.CompareExchange(ref _value, update, expected);
            if (result == null)
            {
                return _defaultValue;
            }

            return (T) result;
        }
    }
}
