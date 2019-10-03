// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Common
{
    public class AtomicReference<T> where T : class
    {
        private T value;
        private readonly T defaultValue;

        public AtomicReference(T initialValue)
        {
            value = initialValue;
            defaultValue = default(T);
        }

        public AtomicReference()
            : this(default(T))
        {
        }

        public T Get() => Interlocked.CompareExchange(ref value, defaultValue, defaultValue);

        public T Set(T newValue) => Interlocked.Exchange(ref value, newValue);

        public T CompareAndSet(T expected, T update)
            => Interlocked.CompareExchange(ref value, update, expected);
    }
}
