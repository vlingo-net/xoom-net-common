// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Common.Completion.Continuations;

internal sealed class AndThenScheduledContinuation<TAntecedentResult, TResult> : AndThenContinuation<TAntecedentResult, TResult>, IScheduled<object?>
{
    private readonly TimeSpan _timeout;
    private ICancellable? _cancellable;
    private readonly AtomicBoolean _executed = new AtomicBoolean(false);

    internal AndThenScheduledContinuation(
        BasicCompletes parent,
        BasicCompletes<TAntecedentResult> antecedent,
        TimeSpan timeout,
        Delegate function)
        : this(parent, antecedent, timeout, Optional.Empty<TResult>(), function)
    {
    }
        
    internal AndThenScheduledContinuation(
        BasicCompletes? parent,
        BasicCompletes<TAntecedentResult> antecedent,
        TimeSpan timeout,
        Optional<TResult> failedOutcomeValue,
        Delegate function)
        : base(parent, antecedent, failedOutcomeValue, function)
    {
        _timeout = timeout;
        ClearTimer();
        StartTimer();
    }

    internal override bool InnerInvoke(BasicCompletes completedCompletes)
    {
        if (TimedOut.Get() || _executed.Get())
        {
            return false;
        }
            
        base.InnerInvoke(completedCompletes);
        _executed.Set(true);
        return true;
    }

    public void IntervalSignal(IScheduled<object?> scheduled, object? data)
    {
        if (!_executed.Get() && !TimedOut.Get())
        {
            TimedOut.Set(true);
            Parent?.TimedOut.Set(true);
            HasFailedValue.Set(true);
        }
    }

    private void StartTimer()
    {
        if (_timeout.TotalMilliseconds > 0 && Parent?.Scheduler != null)
        {
            _cancellable = Parent?.Scheduler.ScheduleOnce(this, null, TimeSpan.Zero, _timeout);
        }
    }

    private void ClearTimer()
    {
        if (_cancellable != null)
        {
            _cancellable.Cancel();
            _cancellable = null;
        }
    }
}