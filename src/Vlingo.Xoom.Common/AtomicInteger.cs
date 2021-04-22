// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Xoom.Common
{
    public class AtomicInteger
    {
        private int _value;

        public AtomicInteger(int initialValue) => _value = initialValue;

        public int AddAndGet(int delta) => Interlocked.Add(ref _value, delta);

        public void Set(int newValue) => Interlocked.Exchange(ref _value, newValue);

        /// <summary>
        /// Replaces the current value with `update` if the current value is `expect`.
        /// </summary>
        /// <param name="expect">Value to compare with.</param>
        /// <param name="update">New value to replace with.</param>
        /// <returns>Whether the value was successfully updated or not.</returns>
        public bool CompareAndSet(int expect, int update) => Interlocked.CompareExchange(ref _value, update, expect) == expect;

        public int Get() => Interlocked.CompareExchange(ref _value, 0, 0);

        public int GetAndSet(int newValue) => Interlocked.Exchange(ref _value, newValue);

        public int GetAndIncrement() => Interlocked.Increment(ref _value) - 1;

        public int IncrementAndGet() => Interlocked.Increment(ref _value);

        public int GetAndDecrement() => Interlocked.Decrement(ref _value) + 1;

        public int DecrementAndGet() => Interlocked.Decrement(ref _value);
    }
}
