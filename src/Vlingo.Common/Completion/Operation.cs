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

        public virtual void OnError(Exception cause) => EmitError(cause);

        public virtual void OnCompletion() => EmitCompletion();

        public virtual bool HasBeenCompleted => _subscriber.HasBeenCompleted;
        
        public virtual void EmitOutcome(TExposes outcome) => _subscriber.OnOutcome(outcome);

        public virtual void EmitError(Exception cause) => _subscriber.OnError(cause);

        public virtual void EmitCompletion() => _subscriber.OnCompletion();

        public virtual void Subscribe(ISink<TExposes> subscriber) => _subscriber = subscriber;
    }
}