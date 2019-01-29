// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public interface IOutcome<TFailure, TSuccess> where TFailure : Exception
    {
        IOutcome<TFailure, TNextSuccess> AndThen<TNextSuccess>(Func<TSuccess, TNextSuccess> action);

        IOutcome<TNextFailure, TNextSuccess> AndThenTo<TNextFailure, TNextSuccess>(
            Func<TSuccess, IOutcome<TNextFailure, TNextSuccess>> action)
            where TNextFailure : Exception;

        void AtLeastConsume(Action<TSuccess> consumer);

        IOutcome<TFailure, TSuccess> Otherwise(Func<TFailure, TSuccess> action);

        IOutcome<TNextFailure, TNextSuccess> OtherwiseTo<TNextFailure, TNextSuccess>(
            Func<TFailure, IOutcome<TNextFailure, TNextSuccess>> action)
            where TNextFailure : Exception;

        TSuccess Get();

        TSuccess GetOrNull();

        TNextSuccess Resolve<TNextSuccess>(
            Func<TFailure, TNextSuccess> onFailedOutcome,
            Func<TSuccess, TNextSuccess> onSuccessfulOutcome);

        Optional<TSuccess> AsOptional();

        ICompletes<TSuccess> AsCompletes();

        IOutcome<NoSuchElementException, TSuccess> Filter(Func<TSuccess, bool> filterFunction);

        IOutcome<TFailure, Tuple<TSuccess, TSecondSuccess>> AlongWith<TOtherFailure, TSecondSuccess>(
            IOutcome<TOtherFailure, TSecondSuccess> outcome) 
            where TOtherFailure : Exception;
    }
}
