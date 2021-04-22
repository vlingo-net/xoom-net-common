// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public class NoSuchElementException : Exception
    {
        public NoSuchElementException(string message, Exception? inner) : base(message, inner)
        {
        }

        public NoSuchElementException(Exception inner) : this("No such element", inner)
        {
        }
    }
}
