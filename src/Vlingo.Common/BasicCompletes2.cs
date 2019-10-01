using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Vlingo.Common
{
    public abstract class BasicCompletes2
    {
        private readonly Scheduler scheduler;
        protected readonly Delegate Action;    // The body of the function. Might be Action<object>, Action<TState> or Action.  Or possibly a Func.
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
            this.scheduler = scheduler;
            Action = action;
        }

        public virtual bool HasFailed { get; }
        
        internal virtual BasicCompletes2 Antecedent { get; }

        internal virtual void InnerInvoke(BasicCompletes2 completedCompletes)
        {
        }

        internal abstract void HandleFailure();

        internal abstract void HandleException(Exception e);
        
        internal abstract Exception Exception { get; }

        internal Scheduler Scheduler => scheduler;
        
        internal virtual void RegisterContinuation(CompletesContinuation continuation)
        {
            Continuations.Add(continuation);
        }

        internal virtual void RegisterFailureContiuation(CompletesContinuation continuationCompletes)
        {
            this.FailureContinuation = continuationCompletes;
        }
        
        internal virtual void RegisterExceptionContiuation(CompletesContinuation continuationCompletes)
        {
            this.ExceptionContinuation = continuationCompletes;
        }

        internal virtual void UpdateFailure(object outcome)
        {
        }

        protected void AndThenInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new StandardCompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterContinuation(continuation);
        }

        protected void OtherwiseInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new StandardCompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterFailureContiuation(continuation);
        }

        protected void RecoverInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new StandardCompletesContinuation(continuationCompletes);
            continuationCompletes.RegisterExceptionContiuation(continuation);
        }
    }
    
    public class BasicCompletes2<TResult> : BasicCompletes2, ICompletes2<TResult>
    {
        private ManualResetEventSlim outcomeKnown = new ManualResetEventSlim(false);
        protected AtomicBoolean hasFailed = new AtomicBoolean(false);
        protected AtomicBoolean hasException = new AtomicBoolean(false);
        protected internal Optional<TResult> failedOutcomeValue;
        protected internal AtomicReference<Exception> exception = new AtomicReference<Exception>();
        protected TResult result;

        public BasicCompletes2(TResult outcome) : base(default)
        {
            this.result = outcome;
        }

        internal BasicCompletes2(Delegate valueSelector) : base(valueSelector)
        {
        }

        public BasicCompletes2(Scheduler scheduler) : base(scheduler, default)
        {
        }

        public BasicCompletes2(TResult outcome, bool succeeded) : base(default)
        {
            if (succeeded)
            {
                With(outcome);
            }
            else
            {
                failedOutcomeValue = Optional.Of(outcome);
                Failed();
            }
        }

        public ICompletes2<TResult> With(TResult outcome)
        {
            this.result = outcome;
            BasicCompletes2 lastRunContinuation = null;
            foreach (var completesContinuation in Continuations)
            {
                if (completesContinuation is StandardCompletesContinuation continuation)
                {
                    try
                    {
                        if (lastRunContinuation == null)
                        {
                            lastRunContinuation = continuation.completes.Antecedent;
                        }
                        continuation.completes.UpdateFailure(outcome);
                        if (continuation.completes.HasFailed)
                        {
                            continuation.completes.HandleFailure();
                            FailureContinuation?.Run(continuation.completes);
                            break;
                        }

                        continuation.Run(lastRunContinuation);
                        lastRunContinuation = continuation.completes;
                    }
                    catch (InvalidCastException)
                    {
                        throw; // raised by failure continuation
                    }
                    catch (Exception e)
                    {
                        this.HandleException(e);
                        ExceptionContinuation?.Run(continuation.completes);
                        break;
                    }
                }
            }

            TrySetResult();
            
            return this;
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenContinuation<TResult, TNewResult>(this, Optional.Of(failedOutcomeValue), function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(Scheduler, this, timeout, function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(Antecedent.Scheduler, this, timeout, Optional.Of(failedOutcomeValue), consumer);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TResult> AndThenConsume(TResult failedOutcomeValue, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TResult>(this, Optional.Of(failedOutcomeValue), consumer);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TResult> AndThenConsume(TimeSpan timeout, Action<TResult> consumer)
        {
            var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(this.Antecedent.Scheduler, this, timeout, consumer);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TResult> AndThenConsume(Action<TResult> consumer)
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

        public ICompletes2<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, ICompletes2<TNewResult>> function)
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
        
        public ICompletes2<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, ICompletes2<TNewResult>> function)
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

        public ICompletes2<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, ICompletes2<TNewResult>> function)
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

        public ICompletes2<TNewResult> AndThenTo<TNewResult>(Func<TResult, ICompletes2<TNewResult>> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TFailedOutcome> function)
        {
            var continuationCompletes = new OtherwiseContinuation<TFailedOutcome, TFailedOutcome>((BasicCompletes2<TFailedOutcome>)(object)this, function);
            OtherwiseInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TResult> OtherwiseConsume(Action<TResult> consumer)
        {
            var continuationCompletes = new OtherwiseContinuation<TResult, TResult>(this, consumer);
            OtherwiseInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TResult> RecoverFrom(Func<Exception, TResult> function)
        {
            if (hasException.Get())
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

        private TNewResult AwaitInternal<TNewResult>()
        {
            if (CompletesResult is ICompletes2<TNewResult> completes)
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
                    return (TNewResult) Convert.ChangeType(result, typeof(TNewResult));
                }
                catch
                {
                    // should not blow but return actual value
                }
            }

            return default;
        }

        public override bool HasFailed => hasFailed.Get();
        public void Failed()
        {
            With(failedOutcomeValue.Get());
        }

        public bool HasOutcome => result != null;

        public TResult Outcome => this.result;

//        internal override void RegisterContinuation(CompletesContinuation continuation)
//        {
//            continuations.Add(continuation);
//        }

        internal override void HandleFailure()
        {
            hasFailed.Set(true);
        }

        internal override void HandleException(Exception e)
        {
            exception.Set(e);
            hasException.Set(true);
            hasFailed.Set(true);
        }

        internal override Exception Exception => exception.Get();

        private void TrySetResult()
        {
            if (Continuations.Any())
            {
                var lastContinuation = Continuations.Last();
                if (lastContinuation is StandardCompletesContinuation standardCompletesContinuation)
                {
                    if (standardCompletesContinuation.completes is BasicCompletes2<TResult> continuation)
                    {
                        this.result = continuation.Outcome;
                    }
                    
                    if (standardCompletesContinuation.completes is BasicCompletes2 completesContinuation)
                    {
                        this.CompletesResult = completesContinuation.CompletesResult;
                        this.TransformedResult = completesContinuation.TransformedResult;
                        outcomeKnown.Set();
                    }
                }
            }
            
            outcomeKnown.Set();
        }
    }
    
    internal class AndThenContinuation<TAntecedentResult, TResult> : BasicCompletes2<TResult>
    {
        private readonly BasicCompletes2<TAntecedentResult> antecedent;

        internal AndThenContinuation(BasicCompletes2<TAntecedentResult> antecedent, Delegate function) : this(antecedent, Optional.Empty<TResult>(), function)
        {
        }
        
        internal AndThenContinuation(BasicCompletes2<TAntecedentResult> antecedent, Optional<TResult> failedOutcomeValue, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
            this.failedOutcomeValue = failedOutcomeValue;
        }

        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
        {
            if (hasFailed.Get())
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
                //if (completedCompletes is AndThenContinuation<TResult, TResult> andThenContinuation)
                //{
                if (completedCompletes.CompletesResult != null)
                {
                    if (completedCompletes.CompletesResult is ICompletes2<TResult> completesContinuation)
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
                }
                else
                {
                    if (completedCompletes is AndThenContinuation<TResult, TResult> andThenContinuation)
                    {
                        invokableActionInput(andThenContinuation.Outcome);
                    }
                }
                return;   
                //}
            }

            if (Action is Func<TAntecedentResult, ICompletes2<TResult>> funcCompletes)
            {
                CompletesResult = funcCompletes(antecedent.Outcome);
                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                result = function(antecedent.Outcome);
                TransformedResult = result;
                return;
            }

//            if (action is Func<ICompletes2<TAntecedentResult>, object?, TResult> funcWithState)
//            {
//                result = funcWithState(antecedent, m_stateObject);
//                return;
//            }
        }

        internal override BasicCompletes2 Antecedent => antecedent;

        internal override Exception Exception => antecedent.Exception;

        internal override void HandleFailure()
        {
            result = failedOutcomeValue.Get();
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
            hasFailed.Set(hasFailed.Get() || outcome.Equals(failedOutcomeValue.Get()));
        }
    }
    
    internal class OtherwiseContinuation<TAntecedentResult, TResult> : BasicCompletes2<TResult>
    {
        private readonly BasicCompletes2<TAntecedentResult> antecedent;

        internal OtherwiseContinuation(BasicCompletes2<TAntecedentResult> antecedent, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
        }

        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
        {
            if (hasException.Get())
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
                    invokableActionInput(andThenContinuation.failedOutcomeValue.Get());
                    return;   
                }
            }
            
            if (Action is Func<ICompletes2<TAntecedentResult>, TResult> funcCompletes)
            {
                result = funcCompletes(antecedent);
                return;
            }

            if (Action is Func<TAntecedentResult, TResult> function)
            {
                if (completedCompletes is AndThenContinuation<TResult, TAntecedentResult> andThenContinuation)
                {
                    result = function(andThenContinuation.failedOutcomeValue.Get());
                    return;   
                }
            }
            
            base.InnerInvoke(completedCompletes);

//            if (action is Func<ICompletes2<TAntecedentResult>, object?, TResult> funcWithState)
//            {
//                result = funcWithState(antecedent, m_stateObject);
//                return;
//            }

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
    
    internal class RecoverContinuation<TResult> : BasicCompletes2<TResult>
    {
        private readonly BasicCompletes2<TResult> antecedent;
        
        internal RecoverContinuation(BasicCompletes2<TResult> antecedent, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
        }

        internal override void InnerInvoke(BasicCompletes2 completedCompletes)
        {
//            if (action is Func<ICompletes2<Exception>, TResult> funcCompletes)
//            {
//                result = funcCompletes(antecedent);
//                return;
//            }

            if (Action is Func<Exception, TResult> function)
            {
                if (completedCompletes is BasicCompletes2<TResult> basicCompletes)
                {
                    result = function(basicCompletes.Exception);
                    return;   
                }
            }

//            if (action is Func<ICompletes2<TAntecedentResult>, object?, TResult> funcWithState)
//            {
//                result = funcWithState(antecedent, m_stateObject);
//                return;
//            }
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
        private AtomicBoolean executed = new AtomicBoolean(false);
        private AtomicBoolean timedOut = new AtomicBoolean(false);

        internal AndThenScheduledContinuation(
            Scheduler scheduler,
            BasicCompletes2<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Delegate function)
            : base(antecedent, function)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
        }
        
        internal AndThenScheduledContinuation(
            Scheduler scheduler,
            BasicCompletes2<TAntecedentResult> antecedent,
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

    internal abstract class CompletesContinuation
    {
        internal abstract void Run(BasicCompletes2 antecedentCompletes);
    }
    
    internal class StandardCompletesContinuation : CompletesContinuation
    {
        internal readonly BasicCompletes2 completes;

        public StandardCompletesContinuation(BasicCompletes2 completes)
        {
            this.completes = completes;
        }

        internal override void Run(BasicCompletes2 antecedentCompletes)
        {
            this.completes.InnerInvoke(antecedentCompletes);
        }
    }
}