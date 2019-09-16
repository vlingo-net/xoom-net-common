// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion.Sources
{
    public class InMemorySource<T> : ILazySource<T>
    {
        public void EmitOutcome(T outcome)
        {
            throw new NotImplementedException();
        }

        public void EmitError(Exception cause)
        {
            throw new NotImplementedException();
        }

        public void EmitCompletion()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(ISink<T> subscriber)
        {
            throw new NotImplementedException();
        }

        public void Activate()
        {
            throw new NotImplementedException();
        }
    }
}