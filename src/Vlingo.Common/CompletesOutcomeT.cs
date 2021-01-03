// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    /// <summary>
    /// Monad transformer implementation for <see cref="IOutcome{TFailure,TSuccess}"/> nested in <see cref="ICompletes{TResult}"/>
    /// </summary>
    /// <typeparam name="TError">The <see cref="IOutcome{TFailure,TSuccess}"/>'s error type</typeparam>
    /// <typeparam name="T">The <see cref="IOutcome{TFailure,TSuccess}"/>'s success type</typeparam>
    public class CompletesOutcomeT<TError, T> where TError : Exception
    {
        private ICompletes<IOutcome<TError, T>> _value;
        private CompletesOutcomeT(ICompletes<IOutcome<TError, T>> value) => _value = value;
        
        public ICompletes<IOutcome<TError, T>> Value => _value;

        public static CompletesOutcomeT<TNewError, TNewT> Of<TNewError, TNewT>(ICompletes<IOutcome<TNewError, TNewT>> value) where TNewError : Exception 
            => new CompletesOutcomeT<TNewError, TNewT>(value);

        public CompletesOutcomeT<TError, TOutcome> AndThen<TOutcome>(Func<T, TOutcome> function) 
            => new CompletesOutcomeT<TError, TOutcome>(_value.AndThen(outcome => outcome.AndThen(function)));

        public CompletesOutcomeT<TError, TOutcome> AndThenTo<TOutcome>(Func<T, CompletesOutcomeT<TError, TOutcome>> function)
        {
            return new CompletesOutcomeT<TError, TOutcome>(
                _value.AndThenTo(outcome =>
                    outcome.Resolve(
                        f => Completes.WithSuccess(Failure.Of<TError, TOutcome>(f)),
                        s =>
                        {
                            try
                            {
                                return function(outcome.Get())._value;
                            }
                            catch (Exception e)
                            {
                                throw new Exception(
                                    "Unexpected exception thrown getting the value out of a successful Outcome!", e);
                            }
                        })
                ));
        }
    }
}