// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Common
{
    public class AtomicInteger
    {
        private int value;

        public AtomicInteger(int initialValue)
        {
            value = initialValue;
        }

        public int AddAndGet(int delta) => Interlocked.Add(ref value, delta);

        public void Set(int newValue) => Interlocked.Exchange(ref value, newValue);

        /// <summary>
        /// Replaces the current value with `update` if the current value is `expect`.
        /// </summary>
        /// <param name="expect">Value to compare with.</param>
        /// <param name="update">New value to replace with.</param>
        /// <returns>Whether the value was successfully updated or not.</returns>
        public bool CompareAndSet(int expect, int update) => Interlocked.CompareExchange(ref value, update, expect) == expect;

        public int Get() => Interlocked.CompareExchange(ref value, 0, 0);

        public int GetAndSet(int newValue) => Interlocked.Exchange(ref value, newValue);

        public int GetAndIncrement() => Interlocked.Increment(ref value) - 1;

        public int IncrementAndGet() => Interlocked.Increment(ref value);

        public int GetAndDecrement() => Interlocked.Decrement(ref value) + 1;

        public int DecrementAndGet() => Interlocked.Decrement(ref value);
    }
}
