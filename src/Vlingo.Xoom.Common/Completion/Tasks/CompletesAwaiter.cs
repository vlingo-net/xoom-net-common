// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;

namespace Vlingo.Common.Completion.Tasks
{
    public struct CompletesAwaiter<T> : INotifyCompletion
    {
        private readonly ICompletes<T> _completes;

        public CompletesAwaiter(ICompletes<T> completes) => _completes = completes;

        // TODO: calling to continuation will release the `await` before the real outcome is set
        public void OnCompleted(Action continuation) => _completes.AndThenConsume(continuation);

        public bool IsCompleted => _completes.IsCompleted;

        public T GetResult() => _completes.Outcome;
    }
}