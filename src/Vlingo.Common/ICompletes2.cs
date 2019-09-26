// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public interface ICompletes2<TResult>
    {
        ICompletes2<TResult> With(TResult outcome);
        ICompletes2<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        ICompletes2<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        ICompletes2<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function);
        ICompletes2<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function);

//        ICompletes2<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer);
//        ICompletes2<TResult> AndThenConsume(TResult failedOutcomeValue, Action<TResult> consumer);
//        ICompletes2<TResult> AndThenConsume(TimeSpan timeout, Action<TResult> consumer);
//        ICompletes2<TResult> AndThenConsume(Action<TResult> consumer);
//
//        TO AndThenTo<TF, TO>(TimeSpan timeout, TF failedOutcomeValue, Func<TResult, TO> function);
//        TO AndThenTo<TF, TO>(TF failedOutcomeValue, Func<TResult, TO> function);
//        TO AndThenTo<TO>(TimeSpan timeout, Func<TResult, TO> function);
//        TO AndThenTo<TO>(Func<TResult, TO> function);
//
        ICompletes2<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TFailedOutcome> function);
//        ICompletes2<TResult> OtherwiseConsume(Action<TResult> consumer);
//        ICompletes2<TResult> RecoverFrom(Func<Exception, TResult> function);
        TResult Await();
        TResult Await(TimeSpan timeout);
//        bool IsCompleted { get; }
        bool HasFailed { get; }
//        void Failed();
//        bool HasOutcome { get; }
        TResult Outcome { get; }
//        ICompletes2<TResult> Repeat();
//        ICompletes2<TResult> Ready();
    }
}
