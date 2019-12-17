// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Common
{
    /// <summary>
    /// Represents a Void value that cannot be instantiated and it's value is always null.
    /// But can be used with generics.
    /// </summary>
    public class Nothing
    {
        public static Nothing AtAll => null;
    }
}