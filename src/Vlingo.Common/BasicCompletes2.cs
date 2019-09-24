using System;
using System.Collections.Generic;
using System.Linq;

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
        protected TResult result;

        public BasicCompletes2(TResult outcome) : base(default)
        {
            this.result = outcome;
        }

        internal BasicCompletes2(Delegate valueSelector) : base(valueSelector)
        {
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
            throw new NotImplementedException();
        }

        public ICompletes2<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
        {
            var continuationCompletes = new ContinuationCompletesResult<TResult, TNewResult>(this, function);
            // Register the continuation.
            AndThenInternal(continuationCompletes /*, scheduler*/);
            return continuationCompletes;
        }

        public TResult Outcome => this.result;
    }
    
    internal sealed class ContinuationCompletesResult<TAntecedentResult, TResult> : BasicCompletes2<TResult> {
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

        internal override void RegisterContinuation(CompletesContinuation completesContinuation)
        {
            antecedent.continuations.Add(completesContinuation);
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
            // TODO: pass in the scheduler
        }

        internal override void Run(BasicCompletes2 completedCompletes)
        {
            // TODO: check if completedCompletes is not in error
            
            this.completes.InnerInvoke();
        }
    }
}