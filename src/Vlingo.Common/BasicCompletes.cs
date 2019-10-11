// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Common.Completion;
using Vlingo.Common.Completion.Continuations;

namespace Vlingo.Common
{
    public class BasicCompletes<TResult> : BasicCompletes, ICompletes<TResult>
    {
        private readonly ManualResetEventSlim outcomeKnown = new ManualResetEventSlim(false);
        protected readonly AtomicBoolean HasException = new AtomicBoolean(false);
        protected internal Optional<TResult> FailedOutcomeValue;
        protected TResult Result;

        public BasicCompletes(TResult outcome) : this(outcome, true)
        {
        }

        internal BasicCompletes(Delegate valueSelector) : base(valueSelector) => Parent = this;

        public BasicCompletes(Scheduler scheduler) : base(scheduler, default) => Parent = this;

        public BasicCompletes(TResult outcome, bool succeeded) : base(default)
        {
            Parent = this;
            if (succeeded)
            {
                With(outcome);
            }
            else
            {
                FailedOutcomeValue = Optional.Of(outcome);
                Failed();
            }
        }
        
        public virtual ICompletes<TO> With<TO>(TO outcome) => (ICompletes<TO>)With((TResult)(object)outcome);

        public ICompletes<TResult> With(TResult outcome)
        {
            Result = outcome;

            var lastRunContinuation = RunContinuations();

            TrySetResult(lastRunContinuation);
            
            return this;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, Parent, this, timeout, Optional.Of(failedOutcomeValue), function);
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
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, Parent, this, timeout, function);
            Parent.AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(Parent.Scheduler, Parent, this, timeout, Optional.Of(failedOutcomeValue), consumer);
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
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(Parent.Scheduler, Parent, this, timeout, consumer);
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
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, Parent, this, timeout, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return default;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, Parent, this, timeout, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return default;
        }
        
        public ICompletes<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, Optional.Of(failedOutcomeValue), function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, Parent, this, timeout, function);
            Parent.AndThenInternal(continuationCompletes);
            return default;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, Parent, this, timeout, function);
            Parent.AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(Parent, this, function);
            Parent.AndThenInternal(continuationCompletes);
            return default;
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
                function(ExceptionValue.Get());
            }
            var continuationCompletes = new RecoverContinuation<TResult>(this, function);
            Parent.RecoverInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TResult Await()
        {
            try
            {
                outcomeKnown.Wait();
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
                outcomeKnown.Wait();
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
                outcomeKnown.Wait(timeout);
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
                outcomeKnown.Wait(timeout);
            }
            catch
            {
                // should not blow but return actual value
            }

            return AwaitInternal<TNewResult>();
        }

        public bool IsCompleted => outcomeKnown.IsSet;

        public bool HasFailed => HasFailedValue.Get();
        public void Failed()
        {
            if (!HandleFailureInternal(FailedOutcomeValue))
            {
                With(FailedOutcomeValue.Get());   
            }
        }

        public bool HasOutcome => Result != null && !Result.Equals(default(TResult));

        public virtual TResult Outcome => Result;
        
        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (Action is Action invokableAction)
            {
                invokableAction();
            }
            
            if (Action is Action<TResult> invokableActionInput)
            {
//                if (completedCompletes.CompletesResult is ICompletes<TResult> completesContinuation)
//                {
//                    if (completesContinuation.HasOutcome)
//                    {
//                        invokableActionInput(completesContinuation.Outcome);
//                        Result = completesContinuation.Outcome;
//                    }
//                    else
//                    {
//                        completesContinuation.AndThenConsume(v => invokableActionInput(v));
//                    }
//                }
//                else
                {
                    if (completedCompletes is AndThenContinuation<TResult, TResult> andThenContinuation)
                    {
                        invokableActionInput(andThenContinuation.Outcome);
                        Result = andThenContinuation.Outcome;
                    }
                    
                    if (completedCompletes is BasicCompletes<TResult> basicCompletes)
                    {
                        invokableActionInput(basicCompletes.Outcome);
                        Result = basicCompletes.Outcome;
                    }
                }
            }
        }

        private TNewResult AwaitInternal<TNewResult>()
        {
//            if (CompletesResult is ICompletes<TNewResult> completes)
//            {
//                if (completes.HasOutcome)
//                {
//                    return completes.Outcome;
//                }
//
//                return completes.Await();
//            }

            if (HasOutcome)
            {
                try
                {
                    if (TransformedResult != null)
                    {
                        return (TNewResult) Convert.ChangeType(TransformedResult, typeof(TNewResult));
                    }
                    return (TNewResult) Convert.ChangeType(Result, typeof(TNewResult));
                }
                catch
                {
                    // should not blow but return actual value
                }
            }

            return default;
        }
        
        internal override void UpdateFailure(BasicCompletes previousContinuation)
        {
            if (previousContinuation is BasicCompletes<TResult> completes && completes.HasOutcome)
            {
                HasFailedValue.Set(HasFailedValue.Get() || completes.Outcome.Equals(FailedOutcomeValue.Get()));
            }
        }

        internal override void RunContinuationsWhenReady()
        {
            var lastCompletes = RunContinuations();
            TrySetResult(lastCompletes);
        }

        private BasicCompletes RunContinuations()
        {
            BasicCompletes lastRunContinuation = this;
            while (Continuations.TryDequeue(out var continuation))
            {
                try
                {
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
                    Result = continuation.Outcome;
                    outcomeKnown.Set();
                    ReadyToExectue.Set(HasOutcome);
                }
            
                if (lastCompletes is BasicCompletes completesContinuation)
                {
                    //CompletesResult = completesContinuation.CompletesResult;
                    TransformedResult = completesContinuation.TransformedResult;
                    outcomeKnown.Set();
                    ReadyToExectue.Set(HasOutcome);
                }   
            }
            else
            {
                if (lastCompletes is BasicCompletes<TResult> continuation && Continuations.IsEmpty)
                {
                    Result = continuation.FailedOutcomeValue.Get();
                }
                
                outcomeKnown.Set();
                ReadyToExectue.Set(HasOutcome);
            }
        }
        
        private void HandleException(Exception e)
        {
            ExceptionValue.Set(e);
            HasException.Set(true);
            HasFailedValue.Set(true);
        }
        
        private bool HandleFailureInternal(Optional<TResult> outcome)
        {
            if (outcomeKnown.IsSet && HasFailed)
            {
                return true; // already reached below
            }

            bool handle = outcome.Equals(FailedOutcomeValue);

            if (handle)
            {
                HasFailedValue.Set(true);
                outcomeKnown.Set();
            }

            return handle;
        }
    }
}