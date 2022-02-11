// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Vlingo.Xoom.Common.Completion;
using Vlingo.Xoom.Common.Completion.Continuations;
using Vlingo.Xoom.Common.Completion.Tasks;

namespace Vlingo.Xoom.Common;

public class BasicCompletes<TResult> : BasicCompletes, ICompletes<TResult>
{
    private readonly TaskCompletionSource<TResult> _tcs = new TaskCompletionSource<TResult>();
    private readonly AtomicRefValue<TResult> _defaultOutcomeValue = new AtomicRefValue<TResult>();
    protected readonly AtomicRefValue<TResult> OutcomeValue = new AtomicRefValue<TResult>();
    protected internal Optional<TResult> FailedOutcomeValue = Optional.Empty<TResult>();

    public BasicCompletes(TResult outcome) : this(outcome, true, null)
    {
    }

    internal BasicCompletes(Delegate valueSelector, BasicCompletes? parent) : base(valueSelector, parent)
    {
    }

    public BasicCompletes(Scheduler scheduler, BasicCompletes? parent = (BasicCompletes?)null) : base(scheduler, default!, parent)
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

    public virtual ICompletes<TOutput> With<TOutput>(TOutput outcome)
    {
        var completes = With((TResult) (object) outcome!);
        object? completesOutcome = completes.Outcome;
        return new BasicCompletes<TOutput>((TOutput)(completesOutcome ?? outcome!));
    }

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
        var parent = Parent ?? this;
        var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(parent, this, timeout, Optional.Of(failedOutcomeValue), function);
        parent.AndThenInternal(scheduledContinuation);
        return scheduledContinuation;
    }

    public virtual ICompletes<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
    {
        var parent = Parent ?? this;
        var scheduledContinuation = new AndThenContinuation<TResult, TNewResult>(parent, this, Optional.Of(failedOutcomeValue), function);
        parent.AndThenInternal(scheduledContinuation);
        return scheduledContinuation;
    }

    public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
    {
        var parent = Parent ?? this;
        var scheduledContinuation = new AndThenScheduledContinuation<TResult, TNewResult>(parent, this, timeout, function);
        parent.AndThenInternal(scheduledContinuation);
        return scheduledContinuation;
    }

    public virtual ICompletes<TNewResult> AndThen<TNewResult>(Func<TResult, TNewResult> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(parent, this, function);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public virtual ICompletes<TResult> AndThen(Func<TResult, TResult> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TResult>(parent, this, function);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public ICompletes<TResult> AndThenConsume(TimeSpan timeout, TResult failedOutcomeValue, Action<TResult> consumer)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(parent, this, timeout, Optional.Of(failedOutcomeValue), consumer);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public ICompletes<TResult> AndThenConsume(TResult failedOutcomeValue, Action<TResult> consumer)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TResult>(parent, this, Optional.Of(failedOutcomeValue), consumer);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public ICompletes<TResult> AndThenConsume(TimeSpan timeout, Action<TResult> consumer)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenScheduledContinuation<TResult, TResult>(parent, this, timeout, consumer);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public ICompletes<TResult> AndThenConsume(Action<TResult> consumer)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TResult>(parent, this, consumer);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }
        
    public ICompletes<TResult> AndThenConsume(Action consumer)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TResult>(parent, this, consumer);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(parent, this, timeout, Optional.Of(failedOutcomeValue), function);
        parent.AndThenInternal(continuationCompletes);
        return default!;
    }

    public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(parent, this, timeout, Optional.Of(failedOutcomeValue), function);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, TNewResult> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(parent, this, Optional.Of(failedOutcomeValue), function);
        parent.AndThenInternal(continuationCompletes);
        return default!;
    }
        
    public ICompletes<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<TResult, ICompletes<TNewResult>> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(parent, this, Optional.Of(failedOutcomeValue), function);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, TNewResult> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(parent, this, timeout, function);
        parent.AndThenInternal(continuationCompletes);
        return default!;
    }

    public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<TResult, ICompletes<TNewResult>> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenScheduledContinuation<TResult, TNewResult>(parent, this, timeout, function);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public TNewResult AndThenTo<TNewResult>(Func<TResult, TNewResult> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(parent, this, function);
        parent.AndThenInternal(continuationCompletes);
        return default!;
    }

    public ICompletes<TNewResult> AndThenTo<TNewResult>(Func<TResult, ICompletes<TNewResult>> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new AndThenContinuation<TResult, TNewResult>(parent, this, function);
        parent.AndThenInternal(continuationCompletes);
        return continuationCompletes;
    }

    public ICompletes<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TResult> function)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new OtherwiseContinuation<TFailedOutcome, TFailedOutcome>(parent, (BasicCompletes<TFailedOutcome>)(object)this, function);
        parent.OtherwiseInternal(continuationCompletes);
        return continuationCompletes;
    }

    public ICompletes<TResult> OtherwiseConsume(Action<TResult> consumer)
    {
        var parent = Parent ?? this;
        var continuationCompletes = new OtherwiseContinuation<TResult, TResult>(parent, this, consumer);
        parent.OtherwiseInternal(continuationCompletes);
        return this;
    }

    public ICompletes<TResult> RecoverFrom(Func<Exception, TResult> function)
    {
        var parent = Parent ?? this;
        if (parent.HasException.Get())
        {
            function(parent.ExceptionValue.Get()!);
        }
        var continuationCompletes = new RecoverContinuation<TResult>(function, this);
        parent.RecoverInternal(continuationCompletes);
        return this;
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
            var failedAwait = OutcomeKnown.Wait(timeout);
            if (!HasFailedValue.Get())
            {
                HasFailedValue.Set(failedAwait);
            }
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

    public void Failed(Exception exception) => HandleException(exception);

    public bool HasOutcome => OutcomeValue.Get() != null && !OutcomeValue.Get()!.Equals(default(TResult)!);

    public virtual TResult Outcome => OutcomeValue.Get();
        
    public virtual ICompletes<TResult> Repeat() => throw new NotImplementedException();

    public ICompletes<TResult> TimeoutWithin(TimeSpan timeout) => AndThenConsume(timeout, result => { });

    public ICompletes<TResult> UseFailedOutcomeOf(TResult failedOutcomeValue)
    {
        FailedOutcomeValue = new Optional<TResult>(failedOutcomeValue);
        Failed();
        return this;
    }

    public CompletesAwaiter<TResult> GetAwaiter() => new CompletesAwaiter<TResult>(this);
        
    public void SetException(Exception exception)
    {
        // run continuation from CompletesMethodBuilder to unstuck await
        // this will also handle failure condition
        RunContinuations();
    }

    public void SetResult(TResult result) => CompletedWith(result);
        
    public Task<TResult> ToTask() => _tcs.Task;

    public override string ToString() => $"{base.ToString()}: OutcomeValue={Outcome}, HasOutcome={HasOutcome}, HasFailed={HasFailed}";

    internal override bool InnerInvoke(BasicCompletes completedCompletes)
    {
        if (Action is Action invokableAction)
        {
            invokableAction();
            return true;
        }
            
        if (Action is Action<TResult> invokableActionInput)
        {
            if (completedCompletes is BasicCompletes<TResult> basicCompletes)
            {
                invokableActionInput(basicCompletes.Outcome);
                OutcomeValue.Set(basicCompletes.Outcome);
                return true;
            }
        }

        return false;
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
        
    internal override bool CanBeExecuted() => HasOutcome || HasFailed || TimedOut.Get();

    protected virtual void Restore()
    {
    }

    protected override void RunContinuationsWhenReady()
    {
        var lastCompletes = RunContinuations();
        TrySetResult(lastCompletes);
            
        lastCompletes?.OutcomeKnown.Set();
        OutcomeKnown.Set();
    }

    protected override void RunContinuationsWhenReady(BasicCompletes completedContinuation)
    {
        var lastCompletes = RunContinuations(completedContinuation);
        TrySetResult(lastCompletes);
    }

    internal void Restore(CompletesContinuation continuation)
    {
        var parent = Parent ?? this;
        parent.AndThenInternal(continuation);
    }
        
    internal override void HandleException(Exception e)
    {
        ExceptionValue.Set(e);
        HasException.Set(true);
        HasFailedValue.Set(true);
        FailedOutcomeValue = Optional.Of(Outcome);
        _tcs.TrySetException(e);
        OutcomeKnown.Set();
        Parent?.HandleException(e);
    }

    private TNewResult AwaitInternal<TNewResult>()
    {
        if (HasOutcome)
        {
            var outcomeValue = OutcomeValue.Get();
            try
            {
                // can yield Object must implement IConvertible.
                if (TransformedResult != null)
                {
                    return (TNewResult) TransformedResult;
                }
                return (TNewResult)(object) outcomeValue!;
            }
            catch
            {
                if (TransformedResult != null)
                {
                    return (TNewResult) Convert.ChangeType(TransformedResult, typeof(TNewResult));
                }

                var convertedOutcomeValue = Convert.ChangeType(outcomeValue, typeof(TNewResult));
                if (convertedOutcomeValue == null)
                {
                    return default!;
                }
                    
                return (TNewResult) convertedOutcomeValue;
            }
        }

        if (HasFailed)
        {
            var failedOutcomeValue = FailedOutcomeValue.Get();
            try
            {
                return (TNewResult)(object)failedOutcomeValue!;
            }
            catch
            {
                var convertedFailedType = Convert.ChangeType(failedOutcomeValue, typeof(TNewResult));
                if (convertedFailedType == null)
                {
                    return default!;
                }
                    
                return (TNewResult) convertedFailedType;
            }
        }

        return default!;
    }

    private BasicCompletes? RunContinuations() => RunContinuations(this);
        
    private BasicCompletes? RunContinuations(BasicCompletes completedContinuation)
    {
        var parent = Parent ?? this;
        var lastRunContinuation = completedContinuation;
        while (parent.Continuations.Count > 0)
        {
            if (!CanExecute(lastRunContinuation))
            {
                return lastRunContinuation;
            }

            if (!parent.Continuations.TryDequeue(out var continuation))
            {
                return lastRunContinuation;
            }

            try
            {
                parent.BackUp(continuation);
                continuation.Completes.UpdateFailure(lastRunContinuation);
                HasFailedValue.Set(continuation.Completes.HasFailedValue.Get());
                if (continuation.Completes.HasFailedValue.Get() && !parent.HasHandledFailure.Get())
                {
                    if (FailureContinuation != null)
                    {
                        FailureContinuation.Run(continuation.Completes);
                        parent.HasHandledFailure.Set(true);
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
                try
                {
                    ExceptionContinuation?.Run(this);
                    if (ExceptionContinuation != null)
                    {
                        lastRunContinuation = ExceptionContinuation?.Completes;
                    }

                    break;
                }
                catch (Exception innerException)
                {
                    continuation.Completes.HandleException(innerException);
                    if (ExceptionContinuation != null)
                    {
                        lastRunContinuation = ExceptionContinuation?.Completes;
                    }

                    break;
                }
            }
        }

        return lastRunContinuation;
    }

    private bool CanExecute(BasicCompletes lastRunContinuation) => lastRunContinuation.CanBeExecuted() || lastRunContinuation.Continuations.Count > 0;

    private void TrySetResult(BasicCompletes? lastCompletes)
    {
        if (lastCompletes == null)
        {
            return;
        }
            
        if (!lastCompletes.HasFailedValue.Get())
        {
            if (lastCompletes is OtherwiseContinuation<TResult, TResult> otherwiseContinuation)
            {
                otherwiseContinuation.HasFailedValue.Set(true);
                OutcomeValue.Set(otherwiseContinuation.Outcome);
            }
                
            if (lastCompletes is BasicCompletes<TResult> continuation && (continuation.HasOutcome || lastCompletes.OutcomeKnown.IsSet))
            {
                OutcomeValue.Set(continuation.Outcome);
            }
            
            if (lastCompletes is { } completesContinuation)
            {
                completesContinuation.OutcomeKnown.Set();
                TransformedResult = completesContinuation.TransformedResult;
            }   
        }
        else
        {
            if (lastCompletes is BasicCompletes<TResult> continuation)
            {
                if (continuation.FailedOutcomeValue.IsPresent)
                {
                    OutcomeValue.Set(continuation.FailedOutcomeValue.Get());   
                }
                else
                {
                    OutcomeValue.Set(continuation.Outcome);
                }
            }

            if (lastCompletes is RecoverContinuation<TResult> recoverContinuation)
            {
                if (recoverContinuation.Parent is BasicCompletes<TResult> parentContinuation)
                {
                    parentContinuation.OutcomeValue.Set(recoverContinuation.Outcome);
                    parentContinuation.HasFailedValue.Set(true);
                    parentContinuation.OutcomeKnown.Set();
                }
            }
        }
    }

    private bool HandleFailureInternal(Optional<TResult> outcome)
    {
        if (OutcomeKnown.IsSet && HasFailed)
        {
            return true; // already reached below
        }

        var handle = outcome.Equals(FailedOutcomeValue);

        if (handle)
        {
            HasFailedValue.Set(true);
            OutcomeKnown.Set();
            ReadyToExectue.Set(true);
        }

        return handle;
    }

    private void CompletedWith(TResult outcome)
    {
        _defaultOutcomeValue.Set(outcome);
            
        OutcomeValue.Set(outcome);
        _tcs.TrySetResult(outcome);
            
        if (Parent is BasicCompletes<TResult> parent)
        {
            parent.OutcomeValue.Set(outcome);
            parent._tcs.TrySetResult(outcome);
        }

        var lastRunContinuation = RunContinuations();

        ReadyToExectue.Set(HasOutcome);
        
        TrySetResult(lastRunContinuation);

        lastRunContinuation?.OutcomeKnown.Set();
        OutcomeKnown.Set();
    }
}