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

        ICompletes2<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer);
        ICompletes2<TResult> AndThenConsume(TResult failedOutcomeValue, Action<TResult> consumer);
        ICompletes2<TResult> AndThenConsume(TimeSpan timeout, Action<TResult> consumer);
        ICompletes2<TResult> AndThenConsume(Action<TResult> consumer);

        TNewResult AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        TNewResult AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function);
        TNewResult AndThenTo<TNewResult>(Func<TResult, TNewResult> function);
        ICompletes2<TNewResult> AndThenTo<TNewResult>(Func<TResult, ICompletes2<TNewResult>> function);

        ICompletes2<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TFailedOutcome> function);
        ICompletes2<TResult> OtherwiseConsume(Action<TResult> consumer);
        ICompletes2<TResult> RecoverFrom(Func<Exception, TResult> function);
        TResult Await();
        TNewResult Await<TNewResult>();
        TResult Await(TimeSpan timeout);
        TNewResult Await<TNewResult>(TimeSpan timeout);
//        bool IsCompleted { get; }
        bool HasFailed { get; }
        void Failed();
        bool HasOutcome { get; }
        TResult Outcome { get; }
//        ICompletes2<TResult> Repeat();
    }
    
    public static class Completes2
    {
        public static ICompletes2<T> Using<T>(Scheduler scheduler)
        {
            return new BasicCompletes2<T>(scheduler);
        }

        public static ICompletes2<T> WithSuccess<T>(T outcome)
        {
            return new BasicCompletes2<T>(outcome, true);
        }

        public static ICompletes2<T> WithFailure<T>(T outcome)
        {
            return new BasicCompletes2<T>(outcome, false);
        }

        public static ICompletes2<T> WithFailure<T>()
        {
            return new BasicCompletes2<T>(default(T), false);
        }

//        public static ICompletes2<T> RepeatableUsing<T>(Scheduler scheduler)
//        {
//            return new RepeatableCompletes<T>(scheduler);
//        }
//
//        public static ICompletes<T> RepeatableWithSuccess<T>(T outcome)
//        {
//            return new RepeatableCompletes<T>(outcome, true);
//        }
//
//        public static ICompletes<T> RepeatableWithFailure<T>(T outcome)
//        {
//            return new RepeatableCompletes<T>(outcome, false);
//        }
//
//        public static ICompletes<T> RepeatableWithFailure<T>()
//        {
//            return new RepeatableCompletes<T>(default(T), false);
//        }
    }
}
