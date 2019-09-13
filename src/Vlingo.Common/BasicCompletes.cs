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
            state.RegisterWithExecution(Action<T>.With(function), timeout, state);
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
            state.RegisterWithExecution(Action<T>.With(consumer), timeout, state);
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
            if (typeof(ICompletes).IsAssignableFrom(typeof(TO)))
            {
                var genericParameter = typeof(TO).GenericTypeArguments[0];
                var nestedGenericTypeDefinition = typeof(BasicCompletes<>);
                var nestedGenericType = nestedGenericTypeDefinition.MakeGenericType(genericParameter);
                var innerCompletes = (dynamic)Activator.CreateInstance(nestedGenericType, state.Scheduler);
                innerCompletes.state.FailedValue(failedOutcomeValue);
                //innerCompletes.state.FailureAction((BasicCompletes<TO>.Action<TO>)(object)state.FailureActionFunction()); // THIS BLOWS because of binder runtime exception
                state.RegisterWithExecution(Action<T>.With(function, innerCompletes), timeout, state);
                return (TO) innerCompletes;
            }
            
            var nestedCompletes = new BasicCompletes<TO>(state.Scheduler);
            nestedCompletes.state.FailedValue(failedOutcomeValue);
            nestedCompletes.state.FailureAction((BasicCompletes<TO>.Action<TO>)(object)state.FailureActionFunction()); // TODO: this will fails if types doesn't match
            state.RegisterWithExecution(Action<T>.With(function, nestedCompletes), timeout, state);
            return default(TO);
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

        public virtual TO Await<TO>()
        {
            state.Await();
            return (TO)(object)Outcome;
        }

        public virtual TO Await<TO>(TimeSpan timeout)
        {
            if (state.Await(timeout))
            {
                return (TO)(object)Outcome;
            }

            return default(TO);
        }

        public virtual bool IsCompleted => state.IsOutcomeKnown;

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
            protected internal readonly TAct DefaultValue;
            protected internal readonly bool HasDefaultValue;
            private readonly object function;
            private readonly ICompletes nestedCompletes;

            protected internal static Action<TAct> With(object function) => new Action<TAct>(function);

            protected internal static Action<TAct> With(object function, ICompletes nestedCompletes)
                => new Action<TAct>(function, nestedCompletes);

            protected internal static Action<TAct> With(object function, TAct defaultValue, ICompletes nestedCompletes)
                => new Action<TAct>(function, defaultValue, nestedCompletes);

            Action(object function)
            {
                this.function = function;
                this.DefaultValue = default(TAct);
                this.HasDefaultValue = false;
                this.nestedCompletes = null;
            }

            Action(object function, TAct defaultValue)
            {
                this.function = function;
                this.DefaultValue = defaultValue;
                this.HasDefaultValue = true;
                this.nestedCompletes = null;
            }

            Action(object function, ICompletes nestedCompletes)
            {
                this.function = function;
                this.DefaultValue = default(TAct);
                this.HasDefaultValue = false;
                this.nestedCompletes = nestedCompletes;
            }

            Action(object function, TAct defaultValue, ICompletes nestedCompletes)
            {
                this.function = function;
                this.DefaultValue = defaultValue;
                this.HasDefaultValue = true;
                this.nestedCompletes = nestedCompletes;
            }

            public virtual F Function<F>() => (F)function;

            public virtual System.Action<TAct> AsConsumer() => (System.Action<TAct>)function;

            public virtual bool IsConsumer => function.GetType().Name.Contains("Action`1");

            public virtual dynamic AsFunction() => function;

            public virtual bool IsFunction => function.GetType().Name.Contains("Func`2");

            public virtual bool HasNestedCompletes => nestedCompletes != null;

            public virtual ICompletes NestedCompletes => nestedCompletes;
        }

        protected internal interface IActiveState<TActSt>
        {
            void Await();
            bool Await(TimeSpan timeout);
            void BackUp(Action<TActSt> action);
            void CancelTimer();
            void CompletedWith(TActSt outcome);
            bool ExecuteFailureAction();
            bool HasFailed { get; }
            void Failed();
            void FailedValue<F>(Optional<F> failedOutcomeValue);
            TActSt FailedValue();
            void FailureAction(Action<TActSt> action);
            Action<TActSt> FailureActionFunction();
            bool HandleFailure(Optional<TActSt> outcome);
            void ExceptionAction(Func<Exception, TActSt> function);
            void HandleException();
            void HandleException(Exception e);
            bool HasException { get; }
            bool HasOutcome { get; }
            void Outcome(TActSt outcome);
            O Outcome<O>();
            bool IsOutcomeKnown { get; set; }
            bool OutcomeMustDefault { get; }
            void RegisterWithExecution(Action<TActSt> action, TimeSpan timeout, IActiveState<TActSt> state);
            bool IsRepeatable { get; }
            void Repeat();
            void Restore();
            void Restore(Action<TActSt> action);
            Scheduler Scheduler { get; }
            void StartTimer(TimeSpan timeout);
        }

        private class Executables<TExec>
        {
            private AtomicBoolean accessible;
            private ConcurrentQueue<Action<TExec>> actions;
            private AtomicBoolean readyToExecute;

            internal Executables()
            {
                accessible = new AtomicBoolean(false);
                actions = new ConcurrentQueue<Action<TExec>>();
                readyToExecute = new AtomicBoolean(false);
            }

            internal int Count => actions.Count;

            internal void Execute(IActiveState<TExec> state)
            {
                while (true)
                {
                    if(accessible.CompareAndSet(false, true))
                    {
                        readyToExecute.Set(true);
                        ExecuteActions(state);
                        accessible.Set(false);
                        break;
                    }
                }
            }

            internal bool IsReadyToExecute => readyToExecute.Get();

            internal void RegisterWithExecution(Action<TExec> action, TimeSpan timeout, IActiveState<TExec> state)
            {
                while (true)
                {
                    if(accessible.CompareAndSet(false, true))
                    {
                        actions.Enqueue(action);
                        if (IsReadyToExecute)
                        {
                            ExecuteActions(state);
                        }
                        else
                        {
                            state.StartTimer(timeout);
                        }
                        accessible.Set(false);
                        break;
                    }
                }
            }

            internal void Reset()
            {
                readyToExecute.Set(false);
                while (!actions.IsEmpty)
                {
                    actions.TryDequeue(out _);
                }
            }

            internal void Restore(Action<TExec> action)
            {
                actions.Enqueue(action);
            }

            private bool HasActions => !actions.IsEmpty;

            private void ExecuteActions(IActiveState<TExec> state)
            {
                while (HasActions)
                {
                    if(state.HasOutcome && state.HasFailed)
                    {
                        state.ExecuteFailureAction();
                        return;
                    }
                    else if(state.HasException)
                    {
                        state.HandleException();
                        return;
                    }

                    if(!actions.TryDequeue(out var action))
                    {
                        continue;
                    }
                    state.BackUp(action);

                    if (action.HasDefaultValue && state.OutcomeMustDefault)
                    {
                        state.Outcome(action.DefaultValue);
                    }
                    else
                    {
                        try
                        {
                            if (action.IsConsumer)
                            {
                                action.AsConsumer().Invoke(state.Outcome<TExec>());
                            }
                            else if (action.IsFunction)
                            {
                                if (action.HasNestedCompletes)
                                {
                                    ((ICompletes<TExec>)action.AsFunction().Invoke(state.Outcome<TExec>()))
                                        .AndThenConsume(value => action.NestedCompletes.With(value));
                                }
                                else
                                {
                                    state.Outcome(action.AsFunction().Invoke(state.Outcome<TExec>()));
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            state.HandleException(ex);
                            break;
                        }
                    }
                }
            }
        }

        protected internal class BasicActiveState<TBActSt> : IActiveState<TBActSt>, IScheduled<object>
        {
            private ICancellable cancellable;
            private readonly Executables<TBActSt> executables;
            private readonly AtomicBoolean failed;
            private Optional<TBActSt> failedOutcomeValue;
            private Action<TBActSt> failureAction;
            private readonly AtomicReference<Exception> exception;
            private Func<Exception, TBActSt> exceptionAction;
            private readonly AtomicReference<object> outcome;
            private ManualResetEventSlim outcomeKnown;
            private readonly Scheduler scheduler;
            private readonly AtomicBoolean timedOut;

            protected internal BasicActiveState(Scheduler scheduler)
            {
                this.scheduler = scheduler;
                executables = new Executables<TBActSt>();
                failed = new AtomicBoolean(false);
                failedOutcomeValue = Optional.Empty<TBActSt>();
                exception = new AtomicReference<Exception>(null);
                outcome = new AtomicReference<object>(null);
                outcomeKnown = new ManualResetEventSlim(false);
                timedOut = new AtomicBoolean(false);
            }

            protected internal BasicActiveState() : this(null)
            {
            }

            public void Await()
            {
                try
                {
                    outcomeKnown.Wait();
                }
                catch { }
            }

            public bool Await(TimeSpan timeout)
            {
                try
                {
                    return outcomeKnown.Wait(timeout);
                }
                catch
                {
                    return false;
                }
            }

            public virtual void BackUp(Action<TBActSt> action)
            {
                // unused; see RepeatableCompletes
            }

            public void CancelTimer()
            {
                if (cancellable != null)
                {
                    cancellable.Cancel();
                    cancellable = null;
                }
            }

            public void CompletedWith(TBActSt outcome)
            {
                CancelTimer();
                if (!timedOut.Get())
                {
                    this.outcome.Set(outcome);
                }

                executables.Execute(this);
                IsOutcomeKnown = true;
            }

            public bool ExecuteFailureAction()
            {
                if(failureAction != null)
                {
                    var executeFailureAction = failureAction;
                    failureAction = null;
                    failed.Set(true);

                    if (executeFailureAction.IsConsumer)
                    {
                        executeFailureAction.AsConsumer().Invoke((TBActSt)outcome.Get());
                    }
                    else
                    {
                        outcome.Set(executeFailureAction.AsFunction().Invoke((TBActSt)outcome.Get()));
                    }

                    return true;
                }

                return false;
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
                failureAction = action;
                if (IsOutcomeKnown && HasFailed)
                {
                    ExecuteFailureAction();
                }
            }

            public Action<TBActSt> FailureActionFunction() => failureAction;

            public bool HandleFailure(Optional<TBActSt> outcome)
            {
                if (IsOutcomeKnown && HasFailed)
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
                    executables.Reset();
                    this.outcome.Set(failedOutcomeValue.Get());
                    IsOutcomeKnown = true;
                    ExecuteFailureAction();
                }

                return handle;
            }

            public virtual void ExceptionAction(Func<Exception, TBActSt> function)
            {
                exceptionAction = function;
                HandleException();
            }

            public virtual void HandleException()
            {
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
                    executables.Reset();
                    outcome.Set(exceptionAction.Invoke(e));
                    IsOutcomeKnown = true;
                }
            }

            public virtual bool HasException => exception.Get() != null;

            public virtual bool HasOutcome => outcome.Get() != null;

            public virtual void Outcome(TBActSt outcome)
            {
                this.outcome.Set(outcome);
            }

            public virtual O Outcome<O>()
            {
                return (O)(outcome.Get() ?? default(O));
            }

            public bool IsOutcomeKnown
            {
                get
                {
                    return outcomeKnown.IsSet;
                }
                set
                {
                    if (value)
                    {
                        outcomeKnown.Set();
                    }
                    else
                    {
                        outcomeKnown.Reset();
                    }
                }
            }

            public bool OutcomeMustDefault => outcome.Get() == null;

            public void RegisterWithExecution(Action<TBActSt> action, TimeSpan timeout, IActiveState<TBActSt> state)
                => executables.RegisterWithExecution(action, timeout, state);

            public virtual bool IsRepeatable => false;

            public virtual void Repeat()
            {
                throw new NotSupportedException();
            }

            public virtual void Restore()
            {
                // unused; see RepeatableCompletes
            }

            public void Restore(Action<TBActSt> action) => executables.Restore(action);

            public virtual Scheduler Scheduler => scheduler;

            public virtual void StartTimer(TimeSpan timeout)
            {
                if (timeout.TotalMilliseconds > 0 && scheduler != null)
                {
                    // 2ms delayBefore prevents timeout until after return from here
                    cancellable = scheduler.ScheduleOnce(this, null, TimeSpan.FromMilliseconds(2), timeout);
                }
            }

            public void IntervalSignal(IScheduled<object> scheduled, object data)
            {
                if(IsOutcomeKnown || executables.IsReadyToExecute)
                {
                    // do nothing
                }
                else
                {
                    timedOut.Set(true);
                    Failed();
                }
            }

            public override string ToString()
            {
                return "BasicActiveState[actions=" + executables.Count + "]";
            }
        }
    }
}