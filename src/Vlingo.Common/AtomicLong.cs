// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Common
{
    public class AtomicLong
    {
        private long value;

        public AtomicLong(long initialValue)
        {
            value = initialValue;
        }

        public long AddAndGet(long delta) => Interlocked.Add(ref value, delta);

        public void Set(long newValue) => Interlocked.Exchange(ref value, newValue);

        /// <summary>
        /// Replaces the current value with `update` if the current value is `expect`.
        /// </summary>
        /// <param name="expect">Value to compare with.</param>
        /// <param name="update">New value to replace with.</param>
        /// <returns>Whether the value was successfully updated or not.</returns>
        public bool CompareAndSet(long expect, long update) => Interlocked.CompareExchange(ref value, update, expect) == expect;

        public long Get() => Interlocked.CompareExchange(ref value, 0, 0);

        public long GetAndSet(long newValue) => Interlocked.Exchange(ref value, newValue);

        public long GetAndIncrement() => Interlocked.Increment(ref value) - 1;

        public long IncrementAndGet() => Interlocked.Increment(ref value);

        public long GetAndDecrement() => Interlocked.Decrement(ref value) + 1;

        public long DecrementAndGet() => Interlocked.Decrement(ref value);
    }
}
