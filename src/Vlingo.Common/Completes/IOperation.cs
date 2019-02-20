// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Completes
{
    public interface IOperation
    {
        void OnError(Exception ex);
    }

    public interface IOperation<TInput> : IOperation
    {
        void OnOutcome(TInput outcome);
        void OnFailure(TInput outcome);
    }

    public interface IOperation<TInput, TOutput, TNextOutput> : IOperation<TInput>
    {
        void AddSubscriber<TLastOutput>(IOperation<TOutput, TNextOutput, TLastOutput> operation);
    }
}
