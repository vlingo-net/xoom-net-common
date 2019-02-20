// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Vlingo.Common
{
    public class BasicCompletes<T> : ICompletes<T>
    {
        protected readonly IActiveState<T> state;

        public BasicCompletes(Scheduler scheduler) : this(new BasicActiveState<T>(scheduler))
        {
        }

        public BasicCompletes(T outcome, bool succeeded) : this(new BasicActiveState<T>(), outcome, succeeded)
        {
        }

        public BasicCompletes(T outcome) : this(new BasicActiveState<T>(), outcome)
        {
        }

        protected internal BasicCompletes(IActiveState<T> state)
        {
            this.state = state;
        }

        protected internal BasicCompletes(IActiveState<T> state, T outcome, bool succeeded)
        {
            this.state = state;
            if (succeeded)
            {
                this.state.CompletedWith(outcome);
            }
            else
            {
                this.state.FailedValue(Optional.Of(outcome));
                this.state.Failed();
            }
        }

        protected internal BasicCompletes(IActiveState<T> state, T outcome)
        {
            this.state = state;
            this.state.Outcome(outcome);
        }

        private ICompletes<TO> AndThenInternal<TO>(TimeSpan timeout, Optional<TO> failedOutcomeValue, Func<T, TO> function)
        {
            state.FailedValue(failedOutcomeValue);
            state.Action(Action<T>.With(function));
            if (state.IsCompleted)
            {
                state.CompleteActions();
            }
            else
            {
                state.StartTimer(timeout);
            }
            return (ICompletes<TO>)this;
        }

        public virtual ICompletes<TO> AndThen<TO>(TimeSpan timeout, TO failedOutcomeValue, Func<T, TO> function)
            => AndThenInternal(timeout, Optional.Of(failedOutcomeValue), function);

        public virtual ICompletes<TO> AndThen<TO>(TO failedOutcomeValue, Func<T, TO> function)
            => AndThenInternal(TimeSpan.FromMilliseconds(Timeout.Infinite), Optional.Of(failedOutcomeValue), function);

        public virtual ICompletes<TO> AndThen<TO>(TimeSpan timeout, Func<T, TO> function)
            => AndThenInternal(timeout, Optional.Empty<TO>(), function);

        public virtual ICompletes<TO> AndThen<TO>(Func<T, TO> function)
            => AndThenInternal(TimeSpan.FromMilliseconds(Timeout.Infinite), Optional.Empty<TO>(), function);

        private ICompletes<T> AndThenConsumeInternal(TimeSpan timeout, Optional<T> failedOutcomeValue, System.Action<T> consumer)
        {
            state.FailedValue(failedOutcomeValue);
            state.Action(Action<T>.With(consumer));
            if (state.IsCompleted)
            {
                state.CompleteActions();
            }
            else
            {
                state.StartTimer(timeout);
            }
            return this;
        }

        public virtual ICompletes<T> AndThenConsume(TimeSpan timeout, T failedOutcomeValue, System.Action<T> consumer)
            => AndThenConsumeInternal(timeout, Optional.Of(failedOutcomeValue), consumer);

        public virtual ICompletes<T> AndThenConsume(TimeSpan timeout, System.Action<T> consumer)
            => AndThenConsumeInternal(timeout, Optional.Empty<T>(), consumer);

        public virtual ICompletes<T> AndThenConsume(T failedOutcomeValue, System.Action<T> consumer)
            => AndThenConsumeInternal(TimeSpan.FromMilliseconds(Timeout.Infinite), Optional.Of(failedOutcomeValue), consumer);

        public virtual ICompletes<T> AndThenConsume(System.Action<T> consumer)
            => AndThenConsumeInternal(TimeSpan.FromMilliseconds(Timeout.Infinite), Optional.Empty<T>(), consumer);

        private TO AndThenToInternal<TF, TO>(TimeSpan timeout, Optional<TF> failedOutcomeValue, Func<T, TO> function)
        {
            var nestedCompletes = new BasicCompletes<TO>(state.Scheduler);
            nestedCompletes.state.FailedValue(failedOutcomeValue);
            nestedCompletes.state.FailureAction((BasicCompletes<TO>.Action<TO>)(object)state.FailureActionFunction());
            state.Action((Action<T>)(object)BasicCompletes<TO>.Action<TO>.With(function, nestedCompletes));
            if (state.IsCompleted)
            {
                state.CompleteActions();
            }
            else
            {
                state.StartTimer(timeout);
            }
            return (TO)(object)nestedCompletes;
        }

        public virtual TO AndThenTo<TF, TO>(TimeSpan timeout, TF failedOutcomeValue, Func<T, TO> function)
            => AndThenToInternal(timeout, Optional.Of(failedOutcomeValue), function);

        public virtual TO AndThenTo<TF, TO>(TF failedOutcomeValue, Func<T, TO> function)
            => AndThenToInternal(TimeSpan.FromMilliseconds(Timeout.Infinite), Optional.Of(failedOutcomeValue), function);

        public virtual TO AndThenTo<TO>(TimeSpan timeout, Func<T, TO> function)
            => AndThenToInternal(timeout, Optional.Empty<object>(), function);

        public virtual TO AndThenTo<TO>(Func<T, TO> function)
            => AndThenToInternal(TimeSpan.FromMilliseconds(Timeout.Infinite), Optional.Empty<object>(), function);

        public virtual ICompletes<T> Otherwise(Func<T, T> function)
        {
            state.FailureAction(Action<T>.With(function));
            return this;
        }

        public virtual ICompletes<T> OtherwiseConsume(System.Action<T> consumer)
        {
            state.FailureAction(Action<T>.With(consumer));
            return this;
        }

        public virtual ICompletes<T> RecoverFrom(Func<Exception, T> function)
        {
            state.ExceptionAction(function);
            return this;
        }

        public virtual T Await() => Await(TimeSpan.FromMilliseconds(Timeout.Infinite));

        public virtual T Await(TimeSpan timeout)
        {
            if (IsCompleted)
            {
                return Outcome;
            }

            using (var manualResetEvent = new ManualResetEventSlim(false))
            {
                try
                {
                    manualResetEvent.Wait(timeout, state.CompletedSignalToken.Token);
                }
                catch
                {
                    // ignore
                }

                return Outcome;
            }
        }

        public virtual bool IsCompleted => state.IsCompleted;

        public virtual bool HasFailed => state.HasFailed;

        public virtual void Failed()
        {
            With(state.FailedValue());
        }

        public virtual bool HasOutcome => state.HasOutcome;

        public virtual T Outcome => state.Outcome<T>();

        public virtual ICompletes<T> Repeat()
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<TO> With<TO>(TO outcome)
        {
            if (!state.HandleFailure(Optional.Of((T)(object)outcome)))
            {
                state.CompletedWith((T)(object)outcome);
            }

            return (ICompletes<TO>)this;
        }

        protected internal class Action<TAct>
        {
            protected internal readonly TAct defaultValue;
            protected internal readonly bool hasDefaultValue;
            private readonly object function;
            private readonly ICompletes<TAct> nestedCompletes;

            protected internal static Action<TAct> With(object function) => new Action<TAct>(function);

            protected internal static Action<TAct> With(object function, ICompletes<TAct> nestedCompletes)
                => new Action<TAct>(function, nestedCompletes);

            protected internal static Action<TAct> With(object function, TAct defaultValue, ICompletes<TAct> nestedCompletes)
                => new Action<TAct>(function, defaultValue, nestedCompletes);

            Action(object function)
            {
                this.function = function;
                this.defaultValue = default(TAct);
                this.hasDefaultValue = false;
                this.nestedCompletes = null;
            }

            Action(object function, TAct defaultValue)
            {
                this.function = function;
                this.defaultValue = defaultValue;
                this.hasDefaultValue = true;
                this.nestedCompletes = null;
            }

            Action(object function, ICompletes<TAct> nestedCompletes)
            {
                this.function = function;
                this.defaultValue = default(TAct);
                this.hasDefaultValue = false;
                this.nestedCompletes = nestedCompletes;
            }

            Action(object function, TAct defaultValue, ICompletes<TAct> nestedCompletes)
            {
                this.function = function;
                this.defaultValue = defaultValue;
                this.hasDefaultValue = true;
                this.nestedCompletes = nestedCompletes;
            }

            public virtual F Function<F>() => (F)function;

            public virtual System.Action<TAct> AsConsumer() => (System.Action<TAct>)function;

            public virtual bool IsConsumer => (function is System.Action<TAct>);

            public virtual Func<TAct, TAct> AsFunction() => (Func<TAct, TAct>)function;

            public virtual bool IsFunction => (function is Func<TAct, TAct>);

            public virtual bool HasNestedCompletes => nestedCompletes != null;

            public virtual ICompletes<TAct> NestedCompletes => nestedCompletes;
        }

        protected internal interface IActiveState<TActSt>
        {
            bool HasAction { get; }
            void Action(Action<TActSt> action);
            Action<TActSt> Action();
            void CancelTimer();
            bool IsCompleted { get; }
            void CompleteActions();
            void CompletedWith(TActSt outcome);
            bool HasFailed { get; }
            void Failed();
            void FailedValue<F>(Optional<F> failedOutcomeValue);
            TActSt FailedValue();
            void FailureAction(Action<TActSt> action);
            void FailureAction();
            Action<TActSt> FailureActionFunction();
            bool HandleFailure(Optional<TActSt> outcome);
            void ExceptionAction(Func<Exception, TActSt> function);
            void HandleException(Exception e);
            bool HasException { get; }
            bool HasOutcome { get; }
            bool OutcomeMustDefault { get; }
            void Outcome(TActSt outcome);
            O Outcome<O>();
            bool IsRepeatable { get; }
            void Repeat();
            Scheduler Scheduler { get; }
            void StartTimer(TimeSpan timeout);
            CancellationTokenSource CompletedSignalToken { get; }
        }

        protected internal class BasicActiveState<TBActSt> : IActiveState<TBActSt>, IScheduled
        {
            private readonly ConcurrentQueue<Action<TBActSt>> actions;
            private ICancellable cancellable;
            private readonly AtomicBoolean completed;
            private readonly AtomicBoolean completing;
            private readonly AtomicBoolean executingActions;
            private readonly AtomicBoolean failed;
            private Optional<TBActSt> failedOutcomeValue;
            private Action<TBActSt> failureAction;
            private readonly AtomicReference<Exception> exception;
            private Func<Exception, TBActSt> exceptionAction;
            private readonly AtomicReference<object> outcome;
            private readonly Scheduler scheduler;
            private readonly AtomicBoolean timedOut;

            protected internal BasicActiveState(Scheduler scheduler)
            {
                this.scheduler = scheduler;
                actions = new ConcurrentQueue<Action<TBActSt>>();
                completed = new AtomicBoolean(false);
                completing = new AtomicBoolean(false);
                executingActions = new AtomicBoolean(false);
                failed = new AtomicBoolean(false);
                failedOutcomeValue = Optional.Empty<TBActSt>();
                exception = new AtomicReference<Exception>(null);
                outcome = new AtomicReference<object>(null);
                timedOut = new AtomicBoolean(false);
                CompletedSignalToken = new CancellationTokenSource();
            }

            protected internal BasicActiveState() : this(null)
            {
            }

            public virtual bool HasAction => !actions.IsEmpty;

            public virtual void Action(Action<TBActSt> action) => actions.Enqueue(action);

            public virtual Action<TBActSt> Action()
            {
                if (actions.TryDequeue(out Action<TBActSt> act))
                {
                    return act;
                }

                return null;
            }

            public virtual void CancelTimer()
            {
                if (cancellable != null)
                {
                    cancellable.Cancel();
                    cancellable = null;
                }
            }

            public virtual bool IsCompleted => completed.Get();

            public virtual void CompleteActions()
            {
                if (completing.CompareAndSet(false, true))
                {
                    ExecuteActions();
                    SetCompleted();
                    completing.Set(false);
                }
            }

            public virtual void CompletedWith(TBActSt outcome)
            {
                if (completing.CompareAndSet(false, true))
                {
                    CancelTimer();

                    if (!timedOut.Get())
                    {
                        this.outcome.Set(outcome);
                    }

                    ExecuteActions();
                    SetCompleted();
                    completing.Set(false);
                }
            }

            public virtual bool HasFailed => failed.Get();

            public virtual void Failed()
            {
                HandleFailure(failedOutcomeValue);
            }

            public virtual void FailedValue<F>(Optional<F> failedOutcomeValue)
            {
                if (failedOutcomeValue.IsPresent)
                {
                    this.failedOutcomeValue = failedOutcomeValue.Map(x => (TBActSt)(object)x);
                }
            }

            public virtual TBActSt FailedValue() => failedOutcomeValue.Get();

            public virtual void FailureAction(Action<TBActSt> action)
            {
                this.failureAction = action;
                if (IsCompleted && HasFailed)
                {
                    FailureAction();
                }
            }

            public virtual void FailureAction()
            {
                failed.Set(true);
                if (failureAction != null)
                {
                    if (failureAction.IsConsumer)
                    {
                        failureAction.AsConsumer().Invoke((TBActSt)outcome.Get());
                    }
                    else
                    {
                        outcome.Set(failureAction.AsFunction().Invoke((TBActSt)outcome.Get()));
                    }
                }
            }

            public virtual Action<TBActSt> FailureActionFunction() => failureAction;

            public virtual bool HandleFailure(Optional<TBActSt> outcome)
            {
                if (IsCompleted && HasFailed)
                {
                    return true; // already reached below
                }

                var handle = false;
                if (outcome.Equals(failedOutcomeValue))
                {
                    handle = true;
                }

                if (handle)
                {
                    failed.Set(true);
                    ClearQueue(actions);
                    this.outcome.Set(failedOutcomeValue.Get());
                    SetCompleted();
                    FailureAction();
                }
                return handle;
            }

            public virtual void ExceptionAction(Func<Exception, TBActSt> function)
            {
                exceptionAction = function;
                if (HasException)
                {
                    HandleException(exception.Get());
                }
            }

            public virtual void HandleException(Exception e)
            {
                exception.Set(e);
                if (exceptionAction != null)
                {
                    failed.Set(true);
                    ClearQueue(actions);
                    outcome.Set(exceptionAction.Invoke(e));
                    SetCompleted();
                }
            }

            public virtual bool HasException => exception.Get() != null;

            public virtual bool HasOutcome => outcome.Get() != null;

            public virtual bool OutcomeMustDefault => outcome.Get() == null;

            public virtual void Outcome(TBActSt outcome)
            {
                this.outcome.Set(outcome);
            }

            public virtual O Outcome<O>()
            {
                return (O)(outcome.Get() ?? default(O));
            }

            public virtual bool IsRepeatable => false;

            public virtual void Repeat()
            {
                throw new NotSupportedException();
            }

            public virtual Scheduler Scheduler => scheduler;

            public CancellationTokenSource CompletedSignalToken { get; }

            public virtual void StartTimer(TimeSpan timeout)
            {
                if (timeout.TotalMilliseconds > 0 && scheduler != null)
                {
                    // 2ms delayBefore prevents timeout until after return from here
                    cancellable = scheduler.ScheduleOnce(this, null, TimeSpan.FromMilliseconds(2), timeout);
                }
            }

            public virtual void IntervalSignal(IScheduled scheduled, object data)
            {
                timedOut.Set(true);
                Failed();
            }

            public override string ToString()
            {
                return "BasicActiveState[actions=" + actions.Count + "]";
            }

            protected virtual void ExecuteActions()
            {
                executingActions.CompareAndSet(false, true);

                while (HasAction)
                {
                    Action<TBActSt> action = Action();
                    if (action.hasDefaultValue && OutcomeMustDefault)
                    {
                        Outcome(action.defaultValue);
                    }
                    else
                    {
                        try
                        {
                            if (action.IsConsumer)
                            {
                                action.AsConsumer().Invoke((TBActSt)outcome.Get());
                            }
                            else if (action.IsFunction)
                            {
                                if (action.HasNestedCompletes)
                                {
                                    ((ICompletes<TBActSt>)action.AsFunction().Invoke((TBActSt)outcome.Get()))
                                      .AndThenConsume(value => action.NestedCompletes.With(value));
                                }
                                else
                                {
                                    outcome.Set(action.AsFunction().Invoke((TBActSt)outcome.Get()));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            HandleException(e);
                            break;
                        }
                    }
                }
                executingActions.Set(false);
            }

            private void SetCompleted()
            {
                completed.Set(true);
                CompletedSignalToken.Cancel();
            }

            private static void ClearQueue(ConcurrentQueue<Action<TBActSt>> queue)
            {
                while (!queue.IsEmpty && queue.TryDequeue(out _)) ;
            }
        }
    }
}
