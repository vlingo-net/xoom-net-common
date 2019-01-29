// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public interface ICompletes
    {
        ICompletes<O> With<O>(O outcome);
    }

    public interface ICompletes<T> : ICompletes
    {
        ICompletes<T> AndThen(long timeout, T failedOutcomeValue, Func<T, T> function);
        ICompletes<T> AndThen(T failedOutcomeValue, Func<T, T> function);
        ICompletes<T> AndThen(long timeout, Func<T, T> function);
        ICompletes<T> AndThen(Func<T, T> function);

        ICompletes<T> AndThenConsume(long timeout, T failedOutcomeValue, Action<T> consumer);
        ICompletes<T> AndThenConsume(T failedOutcomeValue, Action<T> consumer);
        ICompletes<T> AndThenConsume(long timeout, Action<T> consumer);
        ICompletes<T> AndThenConsume(Action<T> consumer);

        O AndThenTo<F, O>(long timeout, F failedOutcomeValue, Func<T, O> function);
        O AndThenTo<F,O>(F failedOutcomeValue, Func<T, O> function);
        O AndThenTo<O>(long timeout, Func<T, O> function);
        O AndThenTo<O>(Func<T, O> function);

        ICompletes<T> Otherwise(Func<T, T> function);
        ICompletes<T> OtherwiseConsume(Action<T> consumer);
        ICompletes<T> RecoverFrom(Func<Exception, T> function);

        T Await();
        T Await(long timeout);
        bool IsCompleted { get; }
        bool HasFailed { get; }
        void Failed();
        bool HasOutcome { get; }
        T Outcome { get; }
        ICompletes<T> Repeat();
        
    }

    public static class Completes
    {
        public static ICompletes<T> Using<T>(Scheduler scheduler)
        {
            return new BasicCompletes<T>(scheduler);
        }

        public static ICompletes<T> WithSuccess<T>(T outcome)
        {
            return new BasicCompletes<T>(outcome, true);
        }

        public static ICompletes<T> WithFailure<T>(T outcome)
        {
            return new BasicCompletes<T>(outcome, false);
        }

        public static ICompletes<T> WithFailure<T>()
        {
            return new BasicCompletes<T>(default(T), false);
        }

        public static ICompletes<T> RepeatableUsing<T>(Scheduler scheduler)
        {
            return new RepeatableCompletes<T>(scheduler);
        }

        public static ICompletes<T> RepeatableWithSuccess<T>(T outcome)
        {
            return new RepeatableCompletes<T>(outcome, true);
        }

        public static ICompletes<T> RepeatableWithFailure<T>(T outcome)
        {
            return new RepeatableCompletes<T>(outcome, false);
        }

        public static ICompletes<T> RepeatableWithFailure<T>()
        {
            return new RepeatableCompletes<T>(default(T), false);
        }
    }
}
