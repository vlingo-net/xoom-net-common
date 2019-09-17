// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completion
{
    public abstract class Operation<TReceives, TExposes> : ISink<TReceives>, ISource<TExposes>
    {
        private ISink<TExposes> _subscriber;
        
        public abstract void OnOutcome(TReceives receives);

        public void OnError(Exception cause) => EmitError(cause);

        public void OnCompletion() => EmitCompletion();

        public virtual bool HasBeenCompleted => _subscriber.HasBeenCompleted;
        
        public void EmitOutcome(TExposes outcome) => _subscriber.OnOutcome(outcome);

        public void EmitError(Exception cause) => _subscriber.OnError(cause);

        public void EmitCompletion() => _subscriber.OnCompletion();

        public void Subscribe(ISink<TExposes> subscriber) => _subscriber = subscriber;
    }
}