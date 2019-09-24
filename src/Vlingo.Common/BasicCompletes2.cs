using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Vlingo.Common
{
    public class BasicCompletes2
    {
        protected Delegate action;    // The body of the function. Might be Action<object>, Action<TState> or Action.  Or possibly a Func.
        internal List<CompletesContinuation> continuations = new List<CompletesContinuation>();
        
        public BasicCompletes2(Delegate action)
        {
            this.action = action;
        }

        internal virtual void InnerInvoke()
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

        internal virtual void RegisterContinuation(CompletesContinuation continuation)
        {
            continuations.Add(continuation);
        }

        protected void AndThenInternal(BasicCompletes2 continuationCompletes)
        {
            var continuation = new StandardCompletesContinuation(continuationCompletes);
            RegisterContinuation(continuation);
        }
    }
    
    public class BasicCompletes2<TResult> : BasicCompletes2, ICompletes2<TResult>
    {
        private readonly Scheduler scheduler;
        private ManualResetEventSlim outcomeKnown = new ManualResetEventSlim(false);
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
                    continuation.Run(this);
                }   
            }

            TrySetResult();
            
            return this;
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
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
        {
            var scheduledContinuation = new ScheduledContinuationCompletesResult<TResult, TNewResult>(scheduler, this, timeout, function);
            AndThenInternal(scheduledContinuation);
            return scheduledContinuation;
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new ContinuationCompletesResult<TResult, TNewResult>(this, function);
            AndThenInternal(continuationCompletes);
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

        public TResult Outcome => this.result;
        
        internal override void RegisterContinuation(CompletesContinuation continuation)
        {
            continuations.Add(continuation);
        }
    }
    
    internal class ContinuationCompletesResult<TAntecedentResult, TResult> : BasicCompletes2<TResult>
    {
        private readonly BasicCompletes2<TAntecedentResult> antecedent;

        public ContinuationCompletesResult(BasicCompletes2<TAntecedentResult> antecedent, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
        }

        internal override void InnerInvoke()
        {
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
        
        internal override void RegisterContinuation(CompletesContinuation continuation)
        {
            antecedent.RegisterContinuation(continuation);
        }
    }
    
    internal sealed class ScheduledContinuationCompletesResult<TAntecedentResult, TResult> : ContinuationCompletesResult<TAntecedentResult, TResult>, IScheduled<object>
    {
        private readonly Scheduler scheduler;
        private readonly TimeSpan timeout;
        private ICancellable cancellable;
        private AtomicBoolean executed = new AtomicBoolean(false);
        private AtomicBoolean timedOut = new AtomicBoolean(false);

        internal ScheduledContinuationCompletesResult(
            Scheduler scheduler,
            BasicCompletes2<TAntecedentResult> antecedent,
            TimeSpan timeout,
            Delegate function)
            : base(antecedent, function)
        {
            this.scheduler = scheduler;
            this.timeout = timeout;
        }

        internal override void InnerInvoke()
        {
            ClearTimer();
            base.InnerInvoke();
            executed.Set(true);
            StartTimer();
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            if (!executed.Get())
            {
                timedOut.Set(true);
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
        internal abstract void Run(BasicCompletes2 completedCompletes);
    }
    
    internal class StandardCompletesContinuation : CompletesContinuation
    {
        internal readonly BasicCompletes2 completes;

        public StandardCompletesContinuation(BasicCompletes2 completes)
        {
            this.completes = completes;
        }

        internal override void Run(BasicCompletes2 completedCompletes)
        {
            // TODO: check if completedCompletes is not in error
            
            this.completes.InnerInvoke();
        }
    }
}