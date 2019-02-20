using System;

namespace Vlingo.Common.Completes
{
    public class Recover<TInput, TNextOutput> : IOperation<TInput, TInput, TNextOutput>
    {
        private readonly Func<Exception, TInput> mapper;
        private IOperation<TInput> nextOperation;

        public Recover(Func<Exception, TInput> mapper)
        {
            this.mapper = mapper;
        }

        public void AddSubscriber<TLastOutput>(IOperation<TInput, TNextOutput, TLastOutput> operation)
            => nextOperation = operation;

        public void OnError(Exception ex)
        {
            try
            {
                nextOperation.OnOutcome(mapper.Invoke(ex));
            }
            catch(Exception mapperEx)
            {
                var mappingEx = new InvalidOperationException(mapperEx.Message, mapperEx);
                mappingEx.Data["suppressed"] = ex;
                nextOperation.OnError(mappingEx);
            }
        }

        public void OnFailure(TInput outcome) => nextOperation.OnFailure(outcome);

        public void OnOutcome(TInput outcome) => nextOperation.OnOutcome(outcome);
    }
}
