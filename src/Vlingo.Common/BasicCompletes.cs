using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Vlingo.Common
{
    public abstract class BasicCompletes2
    {
        protected readonly Delegate Action;    // The body of the function. Might be Action<object>, Action<TState> or Action.  Or possibly a Func.
        internal readonly Scheduler Scheduler;
        internal readonly List<CompletesContinuation> Continuations = new List<CompletesContinuation>();
        internal CompletesContinuation FailureContinuation;
        internal CompletesContinuation ExceptionContinuation;
        internal object CompletesResult;
        internal object TransformedResult;

        protected BasicCompletes2(Delegate action)
        {
            Action = action;
        }

        protected BasicCompletes2(Scheduler scheduler, Delegate action)
        {
            Scheduler = scheduler;
            Action = action;
        }

        public virtual bool HasFailed { get; } = false;

        internal virtual BasicCompletes2 Antecedent { get; } = null;
        
        internal abstract void InnerInvoke(BasicCompletes2 completedCompletes);

        internal abstract void HandleFailure();

        internal abstract void HandleException(Exception e);
        
        internal abstract Exception Exception { get; }
        
        internal virtual void RegisterContinuation(CompletesContinuation continuation)
        {
            Continuations.Add(continuation);
        }

        internal virtual void RegisterFailureContiuation(CompletesContinuation continuationCompletes)
        {
            FailureContinuation = continuationCompletes;
        }
        
        internal virtual void RegisterExceptionContiuation(CompletesContinuation continuationCompletes)
        {
            ExceptionContinuation = continuationCompletes;
        }

        internal virtual void UpdateFailure(object outcome)
        {
        }

        protected void AndThenInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterContinuation(continuation);
        }

        protected void OtherwiseInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterFailureContiuation(continuation);
        }

        protected void RecoverInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new CompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterExceptionContiuation(continuation);
        }
    }
    
    public class BasicCompletes<TResult> : BasicCompletes2, ICompletes<TResult>
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

        public ICompletes<TResult> With(TResult outcome)
        {
            Result = outcome;
            for (var i = 0; i < Continuations.Count; i++)
            {
                var continuation = Continuations[i];
                try
                {
                    continuation.Completes.UpdateFailure(outcome);
                    if (continuation.Completes.HasFailed)
                    {
                        continuation.Completes.HandleFailure();
                        FailureContinuation?.Run(continuation.Completes);
                        break;
                    }

                    continuation.Run(i == 0 ? continuation.Completes.Antecedent : Continuations[i - 1].Completes);
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

        public TResult Outcome => Result;
        
        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
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
                
                if (lastContinuation.Completes is BasicCompletes2 completesContinuation)
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
                //TrySetResult(FailedOutcomeValue.Get());
                outcomeKnown.Set();
                HandleFailure();
            }

            return handle;
        }
    }
    
    internal class AndThenContinuation<TAntecedentResult, TResult> : BasicCompletes<TResult>
    {
        private readonly BasicCompletes<TAntecedentResult> antecedent;

        internal AndThenContinuation(BasicCompletes<TAntecedentResult> antecedent, Delegate function) : this(antecedent, Optional.Empty<TResult>(), function)
        {
        }
        
        internal AndThenContinuation(BasicCompletes<TAntecedentResult> antecedent, Optional<TResult> failedOutcomeValue, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
            FailedOutcomeValue = failedOutcomeValue;
        }

        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
        {
            if (HasFailedValue.Get())
            {
                return;
            }
            
            base.InnerInvoke(completedCompletes);

            if (Action is Func<TAntecedentResult, ICompletes<TResult>> funcCompletes)
            {
                CompletesResult = funcCompletes(antecedent.Outcome);
                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                Result = function(antecedent.Outcome);
                TransformedResult = Result;
            }
        }

        internal override BasicCompletes2 Antecedent => antecedent;

        internal override Exception Exception => antecedent.Exception;

        internal override void HandleFailure()
        {
            Result = FailedOutcomeValue.Get();
            base.HandleFailure();
            antecedent.HandleFailure();
        }

        internal override void RegisterContinuation(CompletesContinuation continuation) => antecedent.RegisterContinuation(continuation);
        
        internal override void RegisterFailureContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterFailureContiuation(continuationCompletes);

        internal override void RegisterExceptionContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterExceptionContiuation(continuationCompletes);

        internal override void UpdateFailure(object outcome)
        {
            HasFailedValue.Set(HasFailedValue.Get() || outcome.Equals(FailedOutcomeValue.Get()));
        }
    }
    
    internal class OtherwiseContinuation<TAntecedentResult, TResult> : BasicCompletes<TResult>
    {
        private readonly BasicCompletes<TAntecedentResult> antecedent;

        internal OtherwiseContinuation(BasicCompletes<TAntecedentResult> antecedent, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
        }

        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
        {
            if (HasException.Get())
            {
                return;
            }
            
            if (Action is Action invokableAction)
            {
                invokableAction();
                return;
            }
            
            if (Action is Action<TResult> invokableActionInput)
            {
                if (completedCompletes is AndThenContinuation<TResult, TResult> andThenContinuation)
                {
                    invokableActionInput(andThenContinuation.FailedOutcomeValue.Get());
                    return;   
                }
            }
            
            if (Action is Func<ICompletes<TAntecedentResult>, TResult> funcCompletes)
            {
                Result = funcCompletes(antecedent);
                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                if (completedCompletes is AndThenContinuation<TResult, TAntecedentResult> andThenContinuation)
                {
                    Result = function(andThenContinuation.FailedOutcomeValue.Get());
                    return;   
                }
            }
            
            base.InnerInvoke(completedCompletes);

            throw new InvalidCastException("Cannot run 'Otherwise' function. Make sure that expecting type is the same as failedOutcomeValue type");
        }

        internal override BasicCompletes2 Antecedent => antecedent;

        internal override Exception Exception => antecedent.Exception;
        
        internal override void RegisterContinuation(CompletesContinuation continuation) => antecedent.RegisterContinuation(continuation);

        internal override void RegisterFailureContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterFailureContiuation(continuationCompletes);
        
        internal override void RegisterExceptionContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterExceptionContiuation(continuationCompletes);
    }
    
    internal class RecoverContinuation<TResult> : BasicCompletes<TResult>
    {
        private readonly BasicCompletes<TResult> antecedent;
        
        internal RecoverContinuation(BasicCompletes<TResult> antecedent, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
        }

        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
        {
            if (Action is Func<Exception, TResult> function)
            {
                if (completedCompletes is BasicCompletes<TResult> basicCompletes)
                {
                    Result = function(basicCompletes.Exception);
                }
            }
        }
        
        internal override BasicCompletes2 Antecedent => antecedent;

        internal override void RegisterExceptionContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterExceptionContiuation(continuationCompletes);
    }
    
    internal sealed class AndThenScheduledContinuation<TAntecedentResult, TResult> : AndThenContinuation<TAntecedentResult, TResult>, IScheduled<object>
    {
        private readonly Scheduler scheduler;
        private readonly TimeSpan timeout;
        private ICancellable cancellable;
        private readonly AtomicBoolean executed = new AtomicBoolean(false);
        private readonly AtomicBoolean timedOut = new AtomicBoolean(false);

        internal AndThenScheduledContinuation(
            Scheduler scheduler,
            BasicCompletes<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Delegate function)
            : base(antecedent, function)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
        }
        
        internal AndThenScheduledContinuation(
            Scheduler scheduler,
            BasicCompletes<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Optional<TResult> failedOutcomeValue,
            Delegate function)
            : base(antecedent, failedOutcomeValue, function)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
        }

        internal override void RegisterContinuation(CompletesContinuation continuation)
        {
            ClearTimer();
            StartTimer();
            base.RegisterContinuation(continuation);
        }

        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
        {
            if (timedOut.Get())
            {
                return;
            }
            
            base.InnerInvoke(completedCompletes);
            executed.Set(true);
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            if (!executed.Get())
            {
                timedOut.Set(true);
                HandleFailure();
            }
        }

        private void StartTimer()
        {
            if (timeout.TotalMilliseconds > 0 && scheduler != null)
            {
                // 2ms delayBefore prevents timeout until after return from here
                cancellable = scheduler.ScheduleOnce(this, null, TimeSpan.FromMilliseconds(2), timeout);
            }
        }

        private void ClearTimer()
        {
            if (cancellable != null)
            {
                cancellable.Cancel();
                cancellable = null;
            }
        }
    }

    internal class CompletesContinuation
    {
        internal readonly BasicCompletes2 Completes;

        public CompletesContinuation(BasicCompletes2 completes)
        {
            Completes = completes;
        }

        internal void Run(BasicCompletes2 antecedentCompletes)
        {
            Completes.InnerInvoke(antecedentCompletes);
        }
    }
}