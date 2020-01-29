// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
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
        private object value;
        private readonly object defaultValue;

        public AtomicRefValue(T initialValue)
        {
            value = initialValue!;
            defaultValue = default(T)!;
        }

        public AtomicRefValue() : this(default!)
        {
        }

        public T Get() => (T)Interlocked.CompareExchange(ref value, defaultValue, defaultValue);

        public T Set(T newValue) => (T)Interlocked.Exchange(ref value, newValue);

        public T CompareAndSet(T expected, T update)
            => (T)Interlocked.CompareExchange(ref value, update, expected);
    }
}
