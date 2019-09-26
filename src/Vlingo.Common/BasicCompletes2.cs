using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Vlingo.Common
{
    public abstract class BasicCompletes2
    {
        protected Delegate action;    // The body of the function. Might be Action<object>, Action<TState> or Action.  Or possibly a Func.
        internal List<CompletesContinuation> continuations = new List<CompletesContinuation>();
        internal CompletesContinuation failureContinuation;
        
        public BasicCompletes2(Delegate action)
        {
            this.action = action;
        }

        public virtual bool HasFailed { get; }
        
        internal virtual BasicCompletes2 Antecedent { get; }

        internal virtual void InnerInvoke(BasicCompletes2 completedCompletes)
        {
            if (action is Action invokableAction)
            {
                invokableAction();
                return;
            }

//            if (action is Action<object?> actionWithState)
//            {
//                actionWithState(m_stateObject);
//                return;
//            }
        }

        internal abstract void HandleFailure();
        
        internal virtual void RegisterContinuation(CompletesContinuation continuation)
        {
            continuations.Add(continuation);
        }

        internal virtual void RegisterFailureContiuation(CompletesContinuation continuationCompletes)
        {
            this.failureContinuation = continuationCompletes;
        }

        internal virtual void UpdateFailure(object outcome)
        {
        }

        protected void AndThenInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new StandardCompletesContinuation(continuationCompletes);
            RegisterContinuation(continuation);
        }

        protected void OtherwiseInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new StandardCompletesContinuation(continuationCompletes);
            RegisterFailureContiuation(continuation);
        }
    }
    
    public class BasicCompletes2<TResult> : BasicCompletes2, ICompletes2<TResult>
    {
        private readonly Scheduler scheduler;
        private ManualResetEventSlim outcomeKnown = new ManualResetEventSlim(false);
        protected bool isFailed;
        protected TResult result;

        public BasicCompletes2(TResult outcome) : base(default)
        {
            this.result = outcome;
        }

        internal BasicCompletes2(Delegate valueSelector) : base(valueSelector)
        {
        }

        public BasicCompletes2(Scheduler scheduler) : base(default)
        {
            this.scheduler = scheduler;
        }

        public ICompletes2<TResult> With(TResult outcome)
        {
            this.result = outcome;
            foreach (var completesContinuation in continuations)
            {
                if (completesContinuation is StandardCompletesContinuation continuation)
                {
                    continuation.completes.UpdateFailure(outcome);
                    if (continuation.completes.HasFailed)
                    {
                        this.HandleFailure();
                        failureContinuation.Run(continuation.completes);
                        break;
                    }
                    continuation.Run(continuation.completes.Antecedent);
                }   
            }

            TrySetResult();
            
            return this;
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(scheduler, this, timeout, Optional.Of(failedOutcomeValue), function);
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
            var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(scheduler, this, timeout, function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(this, function);
            AndThenInternal(continuationCompletes);
            return continuationCompletes;
        }

        public ICompletes2<TResult> Otherwise(Func<TResult, TResult> function)
        {
            var continuationCompletes = new OtherwiseContinuation<TResult, TResult>(this, function);
            OtherwiseInternal(continuationCompletes);
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

            return result;
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

            return result;
        }

        public override bool HasFailed => isFailed;

        public TResult Outcome => this.result;

//        internal override void RegisterContinuation(CompletesContinuation continuation)
//        {
//            continuations.Add(continuation);
//        }

        internal override void HandleFailure()
        {
            isFailed = true;
        }
        
        private void TrySetResult()
        {
            var lastContinuation = continuations.Last();
            if (lastContinuation is StandardCompletesContinuation standardCompletesContinuation)
            {
                if (standardCompletesContinuation.completes is BasicCompletes2<TResult> continuation)
                {
                    this.result = continuation.Outcome;
                }
            }
            outcomeKnown.Set();
        }
    }
    
    internal class AndThenContinuation<TAntecedentResult, TResult> : BasicCompletes2<TResult>
    {
        protected internal readonly Optional<TResult> failedOutcomeValue;
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
            if (isFailed)
            {
                return;
            }
            
            if (action is Func<ICompletes2<TAntecedentResult>, TResult> funcCompletes)
            {
                result = funcCompletes(antecedent);
                return;
            }

            if (action is Func<TAntecedentResult, TResult> function)
            {
                result = function(antecedent.Outcome);
                return;
            }

//            if (action is Func<ICompletes2<TAntecedentResult>, object?, TResult> funcWithState)
//            {
//                result = funcWithState(antecedent, m_stateObject);
//                return;
//            }
        }

        internal override BasicCompletes2 Antecedent => antecedent;

        internal override void HandleFailure()
        {
            base.HandleFailure();
            antecedent.HandleFailure();
        }

        internal override void RegisterContinuation(CompletesContinuation continuation) => antecedent.RegisterContinuation(continuation);
        
        internal override void RegisterFailureContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterFailureContiuation(continuationCompletes);

        internal override void UpdateFailure(object outcome) => isFailed = outcome.Equals(failedOutcomeValue.Get());
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
            if (action is Func<ICompletes2<TAntecedentResult>, TResult> funcCompletes)
            {
                result = funcCompletes(antecedent);
                return;
            }

            if (action is Func<TAntecedentResult, TResult> function)
            {
                if (completedCompletes is AndThenContinuation<TResult, TAntecedentResult> andThenContinuation)
                {
                    result = function(andThenContinuation.failedOutcomeValue.Get());
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

        internal override void RegisterFailureContiuation(CompletesContinuation continuationCompletes) =>
            antecedent.RegisterFailureContiuation(continuationCompletes);
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