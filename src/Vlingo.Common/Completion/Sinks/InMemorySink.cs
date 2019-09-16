// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Sinks
{
    public class InMemorySink<T> : ISink<T>
    {
        public void OnOutcome(T receives)
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception cause)
        {
            throw new NotImplementedException();
        }

        public void OnCompletion()
        {
            throw new NotImplementedException();
        }

        public bool HasBeenCompleted { get; }
    }
}