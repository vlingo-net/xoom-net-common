// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Threading;
using Vlingo.Common.Completion.Continuations;

namespace Vlingo.Common.Completion
{
    public class BasicCompletes<TResult> : BasicCompletes, ICompletes<TResult>
    {
        private readonly ManualResetEventSlim outcomeKnown = new ManualResetEventSlim(false);
        private readonly AtomicReference<Exception> exception = new AtomicReference<Exception>();
        protected readonly AtomicBoolean HasFailedValue = new AtomicBoolean(false);
        protected readonly AtomicBoolean HasException = new AtomicBoolean(false);
        protected internal Optional<TResult> FailedOutcomeValue;
        protected TResult Result;

        public BasicCompletes(TResult outcome) : base(default)
        {
            Result = outcome;
        }

        internal BasicCompletes(Delegate valueSelector) : base(valueSelector)
        {
        }

        public BasicCompletes(Scheduler scheduler) : base(scheduler, default)
        {
        }

        public BasicCompletes(TResult outcome, bool succeeded) : base(default)
        {
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
        
        public virtual ICompletes<TO> With<TO>(TO outcome)
        {
            return (ICompletes<TO>)With((TResult)(object)outcome);
        }

        public virtual ICompletes<TResult> With(TResult outcome)
        {
            Result = outcome;
            for (var i = 0; i < Continuations.Count; i++)
            {
                var continuation = Continuations[i];
                var previousCompletes = i == 0 ? continuation.Completes.Antecedent : Continuations[i - 1].Completes; 
                try
                {
                    continuation.Completes.UpdateFailure(previousCompletes);
                    if (continuation.Completes.HasFailed)
                    {
                        continuation.Completes.HandleFailure();
                        FailureContinuation?.Run(continuation.Completes);
                        break;
                    }

                    continuation.Run(previousCompletes);
                }
                catch (InvalidCastException)
                {
                    throw; // raised by failure continuation
                }
                catch (Exception e)
                {
                    HandleException(e);
                    ExceptionContinuation?.Run(continuation.Completes);
                    break;
                }
            }

            TrySetResult();
            
            return this;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenContinuation<TResult, TNewResult>(this, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(Antecedent.Scheduler, this, timeout, Optional.Of(failedOutcomeValue), consumer);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(TResult failedOutcomeValue, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TResult>(this, Optional.Of(failedOutcomeValue), consumer);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(TimeSpan timeout, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(this.Antecedent.Scheduler, this, timeout, consumer);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> AndThenConsume(Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TResult>(this, consumer);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(continuationCompletes);
            return default;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(continuationCompletes);
            return default;
        }
        
        public ICompletes<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, function);
            AndThenInternal(continuationCompletes);
            return default;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public TNewResult AndThenTo<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, function);
            AndThenInternal(continuationCompletes);
            return default;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(Func<TResult, ICompletes<TNewResult>> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TFailedOutcome> function)
        {
            var continuationCompletes = new OtherwiseContinuation<TFailedOutcome, TFailedOutcome>((BasicCompletes<TFailedOutcome>)(object)this, function);
            OtherwiseInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> OtherwiseConsume(Action<TResult> consumer)
        {
            var continuationCompletes = new OtherwiseContinuation<TResult, TResult>(this, consumer);
            OtherwiseInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes<TResult> RecoverFrom(Func<Exception, TResult> function)
        {
            if (HasException.Get())
            {
                function(exception.Get());
            }
            var continuationCompletes = new RecoverContinuation<TResult>(this, function);
            RecoverInternal(continuationCompletes);
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

        public override bool HasFailed => HasFailedValue.Get();
        public void Failed()
        {
            if (!HandleFailureInternal(FailedOutcomeValue))
            {
                With(FailedOutcomeValue.Get());   
            }
        }

        public bool HasOutcome => Result != null;

        public virtual TResult Outcome => Result;
        
        internal override void InnerInvoke(BasicCompletes completedCompletes)
        {
            if (Action is Action invokableAction)
            {
                invokableAction();
            }
            
            if (Action is Action<TResult> invokableActionInput)
            {
                if (completedCompletes.CompletesResult is ICompletes<TResult> completesContinuation)
                {
                    if (completesContinuation.HasOutcome)
                    {
                        invokableActionInput(completesContinuation.Outcome);
                    }
                    else
                    {
                        completesContinuation.AndThenConsume(v => invokableActionInput(v));
                    }
                }
                else
                {
                    if (completedCompletes is AndThenContinuation<TResult, TResult> andThenContinuation)
                    {
                        invokableActionInput(andThenContinuation.Outcome);
                    }
                }
            }
        }

        internal override void UpdateFailure(BasicCompletes previousContinuation)
        {
            if (previousContinuation is BasicCompletes<TResult> completes)
            {
                HasFailedValue.Set(HasFailedValue.Get() || completes.Outcome.Equals(FailedOutcomeValue.Get()));
            }
        }

        internal override void HandleFailure()
        {
            HasFailedValue.Set(true);
        }

        internal override void HandleException(Exception e)
        {
            exception.Set(e);
            HasException.Set(true);
            HasFailedValue.Set(true);
        }

        internal override Exception Exception => exception.Get();
        
        private TNewResult AwaitInternal<TNewResult>()
        {
            if (CompletesResult is ICompletes<TNewResult> completes)
            {
                if (completes.HasOutcome)
                {
                    return completes.Outcome;
                }

                return completes.Await();
            }

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

        private void TrySetResult()
        {
            if (Continuations.Any())
            {
                var lastContinuation = Continuations.Last();
                if (lastContinuation.Completes is BasicCompletes<TResult> continuation)
                {
                    Result = continuation.Outcome;
                }
                
                if (lastContinuation.Completes is BasicCompletes completesContinuation)
                {
                    CompletesResult = completesContinuation.CompletesResult;
                    TransformedResult = completesContinuation.TransformedResult;
                    outcomeKnown.Set();
                }
            }
            
            outcomeKnown.Set();
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
                HandleFailure();
            }

            return handle;
        }
    }
}