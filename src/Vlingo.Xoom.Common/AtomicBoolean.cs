// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Xoom.Common
{
    public class AtomicBoolean
    {
        private int _value;

        public AtomicBoolean(bool initialValue) => _value = initialValue ? 1 : 0;

        public bool Get()
        {
            return Interlocked.CompareExchange(ref _value, 0, 0) == 1;
        }

        public void Set(bool update)
        {
            var updateInt = update ? 1 : 0;
            Interlocked.Exchange(ref _value, updateInt);
        }

        /// <summary>
        /// Replaces the current value with `update` if the current value is `expect`.
        /// </summary>
        /// <param name="expect">Value to compare with.</param>
        /// <param name="update">New value to replace with.</param>
        /// <returns>Whether the update was successful or not.</returns>
        public bool CompareAndSet(bool expect, bool update)
        {
            var expectedInt = expect ? 1 : 0;
            var updateInt = update ? 1 : 0;

            var actualInt = Interlocked.CompareExchange(ref _value, updateInt, expectedInt);

            return expectedInt == actualInt;
        }
    }
}
