// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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

        public int Set(int newValue) => Interlocked.Exchange(ref value, newValue);

        /// <summary>
        /// Replaces the current value with `update` if the current value is `expect`.
        /// </summary>
        /// <param name="expect">Value to compare with.</param>
        /// <param name="update">New value to replace with.</param>
        /// <returns>The existing value before update, regardless of whether it is updated or not.</returns>
        public int CompareAndSet(int expect, int update) => Interlocked.CompareExchange(ref value, update, expect);

        public int Get() => Interlocked.CompareExchange(ref value, 0, 0);

        public int GetAndIncrement() => Interlocked.Increment(ref value) - 1;

        public int IncrementAndGet() => Interlocked.Increment(ref value);
    }
}
