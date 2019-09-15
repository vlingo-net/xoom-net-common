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
        ICompletes<TO> With<TO>(TO outcome);
    }

    public interface ICompletes<T> : ICompletes
    {
        ICompletes<TO> AndThen<TO>(TimeSpan timeout, TO failedOutcomeValue, Func<T, TO> function);
        ICompletes<TO> AndThen<TO>(TO failedOutcomeValue, Func<T, TO> function);
        ICompletes<TO> AndThen<TO>(TimeSpan timeout, Func<T, TO> function);
        ICompletes<TO> AndThen<TO>(Func<T, TO> function);

        ICompletes<T> AndThenConsume(TimeSpan timeout, T failedOutcomeValue, Action<T> consumer);
        ICompletes<T> AndThenConsume(T failedOutcomeValue, Action<T> consumer);
        ICompletes<T> AndThenConsume(TimeSpan timeout, Action<T> consumer);
        ICompletes<T> AndThenConsume(Action<T> consumer);

        TO AndThenTo<TF, TO>(TimeSpan timeout, TF failedOutcomeValue, Func<T, TO> function);
        TO AndThenTo<TF, TO>(TF failedOutcomeValue, Func<T, TO> function);
        TO AndThenTo<TO>(TimeSpan timeout, Func<T, TO> function);
        TO AndThenTo<TO>(Func<T, TO> function);

        ICompletes<T> Otherwise(Func<T, T> function);
        ICompletes<T> OtherwiseConsume(Action<T> consumer);
        ICompletes<T> RecoverFrom(Func<Exception, T> function);

        TO Await<TO>();
        TO Await<TO>(TimeSpan timeout);
        bool IsCompleted { get; }
        bool HasFailed { get; }
        void Failed();
        bool HasOutcome { get; }
        T Outcome { get; }
        ICompletes<T> Repeat();
        ICompletes<T> Ready();
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
