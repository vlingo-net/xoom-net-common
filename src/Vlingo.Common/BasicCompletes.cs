// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Completion;
using Vlingo.Common.Completion.Continuations;

namespace Vlingo.Common
{
    public class BasicCompletes<TResult> : BasicCompletes, ICompletes<TResult>
    {
        protected readonly AtomicBoolean HasException = new AtomicBoolean(false);
        protected internal Optional<TResult> FailedOutcomeValue = Optional.Empty<TResult>();
        protected AtomicRefValue<TResult> OutcomeValue = new AtomicRefValue<TResult>();

        public BasicCompletes(TResult outcome) : this(outcome, true, null)
        {
        }

        internal BasicCompletes(Delegate valueSelector, BasicCompletes? parent) : base(valueSelector, parent)
        {
        }

        public BasicCompletes(Scheduler scheduler) : this(scheduler, null)
        {
        }
        
        public BasicCompletes(Scheduler scheduler, BasicCompletes? parent) : base(scheduler, default!, parent)
        {
        }

        public BasicCompletes(TResult outcome, bool succeeded) : this(outcome, succeeded, null)
        {
        }

        public BasicCompletes(TResult outcome, bool succeeded, BasicCompletes? parent) : base(default!, parent)
        {
            if (succeeded)
            {
                CompletedWith(outcome);
            }
            else
            {
                FailedOutcomeValue = Optional.Of(outcome);
                Failed();
            }
        }
        
        public virtual ICompletes<TO> With<TO>(TO outcome) => (ICompletes<TO>)With((TResult)(object)outcome!);

        public virtual ICompletes<TResult> With(TResult outcome)
        {
            if (!HandleFailureInternal(Optional.Of(outcome)))
            {
                CompletedWith(outcome);
            }

            return this;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Parent, this, timeout, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenContinuation<TResult, TNewResult>(Parent, this, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Parent, this, timeout, function);
            Parent.AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public virtual ICompletes<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(Parent, this, timeout, Optional.Of(failedOutcomeValue), consumer);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(TResult failedOutcomeValue, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TResult>(Parent, this, Optional.Of(failedOutcomeValue), consumer);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(TimeSpan timeout, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(Parent, this, timeout, consumer);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TResult>(Parent, this, consumer);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Parent, this, timeout, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return default!;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Parent, this, timeout, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return default!;
        }
        
        public ICompletes<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Parent, this, timeout, function);
            Parent.AndThenInternal(continuationCompletes);
            return default!;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Parent, this, timeout, function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, function);
            Parent.AndThenInternal(continuationCompletes);
            return default!;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TFailedOutcome> function)
        {
            var continuationCompletes = new OtherwiseContinuation<TFailedOutcome, TFailedOutcome>(Parent, (BasicCompletes<TFailedOutcome>)(object)this, function);
            Parent.OtherwiseInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> OtherwiseConsume(Action<TResult> consumer)
        {
            var continuationCompletes = new OtherwiseContinuation<TResult, TResult>(Parent, this, consumer);
            Parent.OtherwiseInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> RecoverFrom(Func<Exception, TResult> function)
        {
            if (HasException.Get())
            {
                function(ExceptionValue.Get()!);
            }
            var continuationCompletes = new RecoverContinuation<TResult>(this, function);
            Parent.RecoverInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TResult Await()
        {
            try
            {
                OutcomeKnown.Wait();
            }
            catch
            {
                // should not blow but return actual value
            }

            return AwaitInternal<TResult>();
        }
        
        public TNewResult Await<TNewResult>()
        {
            try
            {
                OutcomeKnown.Wait();
            }
            catch
            {
                // should not blow but return actual value
            }

            return AwaitInternal<TNewResult>();
        }

        public TResult Await(TimeSpan timeout)
        {
            try
            {
                OutcomeKnown.Wait(timeout);
            }
            catch
            {
                // should not blow but return actual value
            }

            return AwaitInternal<TResult>();
        }

        public TNewResult Await<TNewResult>(TimeSpan timeout)
        {
            try
            {
                OutcomeKnown.Wait(timeout);
            }
            catch
            {
                // should not blow but return actual value
            }

            return AwaitInternal<TNewResult>();
        }

        public bool IsCompleted => OutcomeKnown.IsSet;

        public bool HasFailed => HasFailedValue.Get();
        public void Failed()
        {
            if (!HandleFailureInternal(FailedOutcomeValue))
            {
                With(FailedOutcomeValue.Get());   
            }
        }

        public bool HasOutcome => OutcomeValue.Get() != null && !OutcomeValue.Get()!.Equals(default(TResult)!);

        public virtual TResult Outcome => OutcomeValue.Get();
        
        public virtual ICompletes<TResult> Repeat()
        {
            throw new NotImplementedException();
        }

        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (Action is Action invokableAction)
            {
                invokableAction();
            }
            
            if (Action is Action<TResult> invokableActionInput)
            {
                if (completedCompletes is BasicCompletes<TResult> basicCompletes)
                {
                    invokableActionInput(basicCompletes.Outcome);
                    OutcomeValue.Set(basicCompletes.Outcome);
                }
            }
        }

        internal override void UpdateFailure(BasicCompletes previousContinuation)
        {
            if (previousContinuation is BasicCompletes<TResult> completes && completes.HasOutcome)
            {
                HasFailedValue.Set(HasFailedValue.Get() || completes.Outcome!.Equals(FailedOutcomeValue.Get()));
            }
        }
        
        internal override void BackUp(CompletesContinuation continuation)
        {
        }
        
        protected virtual void Restore()
        {
        }

        protected override void RunContinuationsWhenReady()
        {
            var lastCompletes = RunContinuations();
            TrySetResult(lastCompletes);
        }
        
        internal void Restore(CompletesContinuation continuation)
        {
            Parent.AndThenInternal(continuation);
        }

        private TNewResult AwaitInternal<TNewResult>()
        {
            if (HasOutcome)
            {
                try
                {
                    // can yield Object must implement IConvertible.
                    if (TransformedResult != null)
                    {
                        return (TNewResult) TransformedResult;
                    }
                    return (TNewResult)(object) OutcomeValue.Get();
                }
                catch
                {
                    if (TransformedResult != null)
                    {
                        return (TNewResult) Convert.ChangeType(TransformedResult, typeof(TNewResult));
                    }
                    return (TNewResult) Convert.ChangeType(OutcomeValue.Get(), typeof(TNewResult));
                }
            }

            return default!;
        }

        private BasicCompletes RunContinuations()
        {
            BasicCompletes lastRunContinuation = this;
            while (Continuations.TryDequeue(out var continuation))
            {
                try
                {
                    Parent.BackUp(continuation);
                    continuation.Completes.UpdateFailure(lastRunContinuation);
                    HasFailedValue.Set(continuation.Completes.HasFailedValue.Get());
                    if (continuation.Completes.HasFailedValue.Get())
                    {
                        if (FailureContinuation != null)
                        {
                            FailureContinuation.Run(continuation.Completes);
                            return FailureContinuation.Completes;   
                        }

                        return continuation.Completes;
                    }

                    continuation.Run(lastRunContinuation);
                    lastRunContinuation = continuation.Completes;
                }
                catch (InvalidCastException)
                {
                    throw; // raised by failure continuation
                }
                catch (Exception e)
                {
                    HandleException(e);
                    ExceptionContinuation?.Run(this);
                    break;
                }
            }

            return lastRunContinuation;
        } 

        private void TrySetResult(BasicCompletes lastCompletes)
        {
            if (!lastCompletes.HasFailedValue.Get())
            {
                if (lastCompletes is BasicCompletes<TResult> continuation && continuation.HasOutcome)
                {
                    OutcomeValue.Set(continuation.Outcome);
                }
            
                if (lastCompletes is BasicCompletes completesContinuation)
                {
                    TransformedResult = completesContinuation.TransformedResult;
                }   
            }
            else
            {
                if (lastCompletes is BasicCompletes<TResult> continuation)
                {
                    OutcomeValue.Set(continuation.FailedOutcomeValue.Get());
                }
            }
        }
        
        private void HandleException(Exception e)
        {
            ExceptionValue.Set(e);
            HasException.Set(true);
            HasFailedValue.Set(true);
            FailedOutcomeValue = Optional.Of(Outcome);
        }
        
        private bool HandleFailureInternal(Optional<TResult> outcome)
        {
            if (OutcomeKnown.IsSet && HasFailed)
            {
                return true; // already reached below
            }

            bool handle = outcome.Equals(FailedOutcomeValue);

            if (handle)
            {
                HasFailedValue.Set(true);
            }

            return handle;
        }

        private void CompletedWith(TResult outcome)
        {
            if (!TimedOut.Get())
            {
                OutcomeValue.Set(outcome);
            }
            
            var lastRunContinuation = RunContinuations();
            ReadyToExectue.Set(HasOutcome);
            
            TrySetResult(lastRunContinuation);
            
            OutcomeKnown.Set();
        }
    }
}