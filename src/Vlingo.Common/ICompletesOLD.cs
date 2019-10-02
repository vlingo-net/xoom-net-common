// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common
{
    public interface ICompletesOLD
    {
        ICompletesOLD<TO> With<TO>(TO outcome);
    }

    public interface ICompletesOLD<T> : ICompletesOLD
    {
        ICompletesOLD<TO> AndThen<TO>(TimeSpan timeout, TO failedOutcomeValue, Func<T, TO> function);
        ICompletesOLD<TO> AndThen<TO>(TO failedOutcomeValue, Func<T, TO> function);
        ICompletesOLD<TO> AndThen<TO>(TimeSpan timeout, Func<T, TO> function);
        ICompletesOLD<TO> AndThen<TO>(Func<T, TO> function);

        ICompletesOLD<T> AndThenConsume(TimeSpan timeout, T failedOutcomeValue, Action<T> consumer);
        ICompletesOLD<T> AndThenConsume(T failedOutcomeValue, Action<T> consumer);
        ICompletesOLD<T> AndThenConsume(TimeSpan timeout, Action<T> consumer);
        ICompletesOLD<T> AndThenConsume(Action<T> consumer);

        TO AndThenTo<TF, TO>(TimeSpan timeout, TF failedOutcomeValue, Func<T, TO> function);
        TO AndThenTo<TF, TO>(TF failedOutcomeValue, Func<T, TO> function);
        TO AndThenTo<TO>(TimeSpan timeout, Func<T, TO> function);
        TO AndThenTo<TO>(Func<T, TO> function);

        ICompletesOLD<T> Otherwise(Func<T, T> function);
        ICompletesOLD<T> OtherwiseConsume(Action<T> consumer);
        ICompletesOLD<T> RecoverFrom(Func<Exception, T> function);

        TO Await<TO>();
        TO Await<TO>(TimeSpan timeout);
        bool IsCompleted { get; }
        bool HasFailed { get; }
        void Failed();
        bool HasOutcome { get; }
        T Outcome { get; }
        ICompletesOLD<T> Repeat();
        ICompletesOLD<T> Ready();
    }

    public static class CompletesOLD
    {
        public static ICompletesOLD<T> Using<T>(Scheduler scheduler)
        {
            return new BasicCompletesOLD<T>(scheduler);
        }

        public static ICompletesOLD<T> WithSuccess<T>(T outcome)
        {
            return new BasicCompletesOLD<T>(outcome, true);
        }

        public static ICompletesOLD<T> WithFailure<T>(T outcome)
        {
            return new BasicCompletesOLD<T>(outcome, false);
        }

        public static ICompletesOLD<T> WithFailure<T>()
        {
            return new BasicCompletesOLD<T>(default(T), false);
        }

        public static ICompletesOLD<T> RepeatableUsing<T>(Scheduler scheduler)
        {
            return new RepeatableCompletes<T>(scheduler);
        }

        public static ICompletesOLD<T> RepeatableWithSuccess<T>(T outcome)
        {
            return new RepeatableCompletes<T>(outcome, true);
        }

        public static ICompletesOLD<T> RepeatableWithFailure<T>(T outcome)
        {
            return new RepeatableCompletes<T>(outcome, false);
        }

        public static ICompletesOLD<T> RepeatableWithFailure<T>()
        {
            return new RepeatableCompletes<T>(default(T), false);
        }
    }
}
