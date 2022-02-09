// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;

namespace Vlingo.Xoom.Common.Completion.Tasks
{
    public sealed class CompletesMethodBuilder<T>
    {
        private BasicCompletes<T>? _completes;
        
        public static CompletesMethodBuilder<T> Create() => new CompletesMethodBuilder<T>();

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();

        public ICompletes<T> Task => _completes ??= new BasicCompletes<T>(new Scheduler());
        
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => awaiter.OnCompleted(stateMachine.MoveNext);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => awaiter.UnsafeOnCompleted(stateMachine.MoveNext);

        public void SetResult(T result) =>  Task.SetResult(result);
        
        public void SetException(Exception e) => Task.SetException(e);

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        { 
            // nothing to do
        }
    }
}