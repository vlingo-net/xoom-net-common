// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    /// <summary>
    /// Represents a outcome of a process that can fail with an unexpected exception. Outcomes can be
    /// mapped and composed safely, converted to Java standard Optional, to null and to a Vlingo Completes.
    /// </summary>
    /// <typeparam name="TFailure">The type of an unexpection exception.</typeparam>
    /// <typeparam name="TSuccess">The type of the expected value.</typeparam>
    public interface IOutcome<TFailure, TSuccess> where TFailure : Exception
    {
        /// <summary>
        /// Maps a success value to the next success value.
        /// <para>
        /// For example:
        /// <code>
        /// Success.Of&lt;Exception, int&gt;(42).AndThen(v =&gt; v + 8).Get(); // 50
        /// </code>
        /// </para>
        /// In case that the outcome is failure, nothing will happen.
        /// </summary>
        /// <typeparam name="TNextSuccess">Return type of the <paramref name="action"/>.</typeparam>
        /// <param name="action">The function to apply to the current value.</param>
        /// <returns>A successful IOutcome with the new value, or a Failure outcome.</returns>
        IOutcome<TFailure, TNextSuccess> AndThen<TNextSuccess>(Func<TSuccess, TNextSuccess> action);

        /// <summary>
        /// Maps a success Outcome value to a function that returns a new Outcome, that can be
        /// either successful or failure.
        /// </summary>
        /// <typeparam name="TNextFailure">The type of the outcome on failure.</typeparam>
        /// <typeparam name="TNextSuccess">The type of the outcome on success.</typeparam>
        /// <param name="action">Function to apply to the current value.</param>
        /// <returns></returns>
        IOutcome<TNextFailure, TNextSuccess> AndThenTo<TNextFailure, TNextSuccess>(
            Func<TSuccess, IOutcome<TNextFailure, TNextSuccess>> action)
            where TNextFailure : Exception;

        /// <summary>
        /// Consumes eventually the successful outcome.
        /// </summary>
        /// <param name="consumer">A consumer action that processes the outcome.</param>
        void AtLeastConsume(Action<TSuccess> consumer);

        /// <summary>
        /// Maps a failure outcome to a successful outcome, for recovery.
        /// </summary>
        /// <param name="action">The function to apply to the current value.</param>
        /// <returns>A successful outcome.</returns>
        IOutcome<TFailure, TSuccess> Otherwise(Func<TFailure, TSuccess> action);

        /// <summary>
        /// Maps a failure outcome to a new outcome.
        /// </summary>
        /// <typeparam name="TNextFailure">The type of the outcome on failure.</typeparam>
        /// <typeparam name="TNextSuccess">The type of the outcome on success.</typeparam>
        /// <param name="action">The function to apply to current value.</param>
        /// <returns></returns>
        IOutcome<TNextFailure, TNextSuccess> OtherwiseTo<TNextFailure, TNextSuccess>(
            Func<TFailure, IOutcome<TNextFailure, TNextSuccess>> action)
            where TNextFailure : Exception;

        /// <summary>
        /// Returns the success outcome value.
        /// </summary>
        /// <returns></returns>
        TSuccess Get();

        /// <summary>
        /// Returns the success outcome value or null in case of a failure.
        /// </summary>
        /// <returns></returns>
        TSuccess GetOrNull();

        /// <summary>
        /// Resolves the outcome and returns the mapped value.
        /// <para>
        /// For example: 
        /// </para>
        /// <para>
        /// <code>
        /// Failure.Of&lt;Exception, int&gt;(exception).Resolve(f =&gt; 42, s =&gt; 1);// 42
        /// </code>
        /// </para>
        /// <para>
        /// <code>
        /// Success.Of&lt;Exception, int&gt;(someValue).Resolve(f =&gt; 42, s =&gt; 1); // 1
        /// </code>
        /// </para>
        /// </summary>
        /// <typeparam name="TNextSuccess">The type of the next success.</typeparam>
        /// <param name="onFailedOutcome">A mapping function from a failure to a success outcome.</param>
        /// <param name="onSuccessfulOutcome">A mapping function from a success outcome to another success outcome.</param>
        /// <returns></returns>
        TNextSuccess Resolve<TNextSuccess>(
            Func<TFailure, TNextSuccess> onFailedOutcome,
            Func<TSuccess, TNextSuccess> onSuccessfulOutcome);

        /// <summary>
        /// An optional with the success value, or Optional.Empty&lt;<typeparamref name="TSuccess"/>&gt;() in case of failure.
        /// </summary>
        /// <returns></returns>
        Optional<TSuccess> AsOptional();

        /// <summary>
        /// A Vlingo Completes with the success value, or a failed Completes in case of failure.
        /// </summary>
        /// <returns></returns>
        ICompletes<TSuccess> AsCompletes();

        /// <summary>
        /// Applies a filter predicate to the success value, or returns a failed Outcome in case
        /// of not fulfilling the predicate.
        /// </summary>
        /// <param name="filterFunction">The filter function.</param>
        /// <returns></returns>
        IOutcome<NoSuchElementException, TSuccess> Filter(Func<TSuccess, bool> filterFunction);

        /// <summary>
        /// Returns a Outcome of a tuple of successes, or the first Failure in case of any of the failed outcomes.
        /// </summary>
        /// <typeparam name="TOtherFailure">The type of failure of the <paramref name="outcome"/>.</typeparam>
        /// <typeparam name="TSecondSuccess">The type of the second success.</typeparam>
        /// <param name="outcome">The outcome.</param>
        /// <returns></returns>
        IOutcome<TFailure, Tuple<TSuccess, TSecondSuccess>> AlongWith<TOtherFailure, TSecondSuccess>(
            IOutcome<TOtherFailure, TSecondSuccess> outcome) 
            where TOtherFailure : Exception;

        /// <summary>
        /// Maps a failed outcome to another failed outcome.
        /// </summary>
        /// <typeparam name="TNextFailure">The type of the next failure.</typeparam>
        /// <param name="action">The function to map a failed outcome to another failed outcome.</param>
        /// <returns>The new failed outcome.</returns>
        IOutcome<TNextFailure, TSuccess> OtherwiseFail<TNextFailure>(Func<TFailure, TNextFailure> action)
            where TNextFailure : Exception;
    }
}
