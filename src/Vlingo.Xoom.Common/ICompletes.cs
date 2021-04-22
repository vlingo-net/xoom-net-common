// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using Vlingo.Xoom.Common.Completion.Tasks;

namespace Vlingo.Xoom.Common
{
    public interface ICompletes
    {
        ICompletes<TOutput> With<TOutput>(TOutput outcome);
    }
    
    [AsyncMethodBuilder(typeof(CompletesMethodBuilder<>))]
    public interface ICompletes<TResult> : ICompletes
    {
        ICompletes<TResult> With(TResult outcome);
        ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        ICompletes<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function);
        ICompletes<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function);
        ICompletes<TResult> AndThen(Func<TResult, TResult> function);

        ICompletes<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer);
        ICompletes<TResult> AndThenConsume(TResult failedOutcomeValue, Action<TResult> consumer);
        ICompletes<TResult> AndThenConsume(TimeSpan timeout, Action<TResult> consumer);
        ICompletes<TResult> AndThenConsume(Action<TResult> consumer);
        ICompletes<TResult> AndThenConsume(Action consumer);

        TNewResult AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function);
        TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function);
        ICompletes<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function);
        TNewResult AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function);
        ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, ICompletes<TNewResult>> function);
        TNewResult AndThenTo<TNewResult>(Func<TResult, TNewResult> function);
        ICompletes<TNewResult> AndThenTo<TNewResult>(Func<TResult, ICompletes<TNewResult>> function);

        ICompletes<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TResult> function);
        ICompletes<TResult> OtherwiseConsume(Action<TResult> consumer);
        ICompletes<TResult> RecoverFrom(Func<Exception, TResult> function);
        TResult Await();
        TNewResult Await<TNewResult>();
        TResult Await(TimeSpan timeout);
        TNewResult Await<TNewResult>(TimeSpan timeout);
        bool IsCompleted { get; }
        bool HasFailed { get; }
        void Failed();
        void Failed(Exception exception);
        bool HasOutcome { get; }
        TResult Outcome { get; }
        ICompletes<TResult> Repeat();
        
        /// <summary>
        /// Answer myself after registering the <paramref name="timeout"/>.
        /// </summary>
        /// <param name="timeout">The time until this <see cref="ICompletes{TResult}"/> is considered timed out.</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        /// <remarks>
        /// WARNING: If you use this method along with <c>UseFailedOutcomeOf(TFailure)</c>, you must
        /// use this one after to register the <paramref name="timeout"/> threshold. Otherwise the timeout
        /// may occur prior to knowing the proper <c>failedOutcomeValue</c> to set.
        /// </remarks>
        ICompletes<TResult> TimeoutWithin(TimeSpan timeout);

        /// <summary>
        /// Answer myself after registering the <paramref name="failedOutcomeValue"/>.
        /// </summary>
        /// <param name="failedOutcomeValue">The TFailed outcome to use when a failure occurs</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        /// <remarks>
        /// WARNING: If you use this method along with <c>UseFailedOutcomeOf(TFailure)</c>, you must
        /// use this one after to register the <c>timeout</c> threshold. Otherwise the timeout
        /// may occur prior to knowing the proper <paramref name="failedOutcomeValue"/> to set.
        /// </remarks>
        ICompletes<TResult> UseFailedOutcomeOf(TResult failedOutcomeValue);
        CompletesAwaiter<TResult> GetAwaiter();
        void SetException(Exception exception);
        void SetResult(TResult result);
    }

    public static class Completes
    {
        public static ICompletes<T> Using<T>(Scheduler scheduler) => new BasicCompletes<T>(scheduler);

        public static ICompletes<T> WithSuccess<T>(T outcome) => new BasicCompletes<T>(outcome, true);

        public static ICompletes<T> WithFailure<T>(T outcome) => new BasicCompletes<T>(outcome, false);

        public static ICompletes<T> WithFailure<T>() => new BasicCompletes<T>(default!, false);

        public static ICompletes<T> RepeatableUsing<T>(Scheduler scheduler) => new RepeatableCompletes<T>(scheduler);

        public static ICompletes<T> RepeatableWithSuccess<T>(T outcome) => new RepeatableCompletes<T>(outcome, true);

        public static ICompletes<T> RepeatableWithFailure<T>(T outcome) => new RepeatableCompletes<T>(outcome, false);

        public static ICompletes<T> RepeatableWithFailure<T>() => new RepeatableCompletes<T>(default!, false);

        public static ICompletes<IOutcome<TFailure, TSuccess>> Invert<TFailure, TSuccess>(IOutcome<TFailure, ICompletes<TSuccess>> outcome) where TFailure : Exception =>
            outcome.Resolve(
                e => WithSuccess(Failure.Of<TFailure, TSuccess>(e)),
                s => s.AndThen<IOutcome<TFailure, TSuccess>>(Success.Of<TFailure, TSuccess>));
        
        public static ICompletes<TResult> AsTyped<TResult>() => new BasicCompletes<TResult>(default!);
    }
}
