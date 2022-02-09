// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Xoom.Common
{
    public class AtomicReference<T> where T : class
    {
        private T? _value;
        private readonly T? _defaultValue;

        public AtomicReference(T? initialValue)
        {
            _value = initialValue;
            _defaultValue = default;
        }

        public AtomicReference() : this(default)
        {
        }

        public T? Get() => Interlocked.CompareExchange(ref _value, _defaultValue, _defaultValue);

        public T? Set(T? newValue) => Interlocked.Exchange(ref _value, newValue);

        public T? CompareAndSet(T? expected, T? update)
            => Interlocked.CompareExchange(ref _value, update, expected);
    }
}
