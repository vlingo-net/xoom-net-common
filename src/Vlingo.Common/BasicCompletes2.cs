using System;

namespace Vlingo.Common
{
    public class BasicCompletes2<TResult> : ICompletes2<TResult>
    {
        private TResult result;
        private Delegate action;    // The body of the function. Might be Action<object>, Action<TState> or Action.  Or possibly a Func.

        public BasicCompletes2(TResult outcome)
        {
            this.result = outcome;
        }

        public BasicCompletes2(Delegate action)
        {
            this.action = action;
        }
        
        public ICompletes<TO> With<TO>(TO outcome)
        {
            throw new NotImplementedException();
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
            return continuationCompletes;
        }

        public TResult Outcome => this.result;
    }
    
    internal sealed class ContinuationCompletesResult<TAntecedentResult, TResult> : BasicCompletes2<TResult> {
        private readonly ICompletes2<TAntecedentResult> antecedent;

        public ContinuationCompletesResult(ICompletes2<TAntecedentResult> antecedent, Delegate function) : base(function)
        {
            this.antecedent = antecedent;
        }
    }
}