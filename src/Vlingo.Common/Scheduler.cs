// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Vlingo.Common
{
    /// <summary>
    /// Provide time-based notifications to a <code>IScheduled&lt;T&gt;</code> once or any number of
    /// times until cancellation. The implementor of the <code>IScheduled&lt;T&gt;</code> protocol
    /// is not assumed to be an <code>Actor</code> and may be a POCO, but the notifications
    /// are quite effectively used in an Actor-based asynchronous environment.
    /// </summary>
    public class Scheduler: IDisposable
    {
        private bool _disposed;
        private readonly ConcurrentStack<ICancellable> _tasks;
        
        /// <summary>
        /// Answer a <code>ICancellable</code> for the repeating scheduled notifier.
        /// </summary>
        /// <typeparam name="T">The type of data to be sent with each notification.</typeparam>
        /// <param name="scheduled">The <code>IScheduled&lt;T&gt;</code> to receive notification.</param>
        /// <param name="data">The data to be sent with each notification.</param>
        /// <param name="delayBefore">The duration before notification interval timing will begin.</param>
        /// <param name="interval">The duration between each notification.</param>
        /// <returns><code>ICancellable</code></returns>
        public virtual ICancellable Schedule<T>(IScheduled<T> scheduled, T data, TimeSpan delayBefore, TimeSpan interval)
            => CreateAndStore(
                scheduled,
                data,
                delayBefore,
                interval,
                true);

        /// <summary>
        /// Answer a <code>ICancellable</code> for single scheduled notifier.
        /// </summary>
        /// <typeparam name="T">The type of data to be sent with each notification.</typeparam>
        /// <param name="scheduled">The <code>IScheduled&lt;T&gt;</code> to receive notification.</param>
        /// <param name="data">The data to be sent with the notification.</param>
        /// <param name="delayBefore">The duration before notification interval timing will begin.</param>
        /// <param name="interval">The duration before the single notification.</param>
        /// <returns><code>ICancellable</code></returns>
        public virtual ICancellable ScheduleOnce<T>(IScheduled<T> scheduled, T data, TimeSpan delayBefore, TimeSpan interval)
            => CreateAndStore(
                scheduled,
                data,
                delayBefore + interval,
                TimeSpan.FromMilliseconds(Timeout.Infinite),
                false);


        public Scheduler() => _tasks = new ConcurrentStack<ICancellable>();

        public virtual void Close()
        {
            foreach(var task in _tasks)
            {
                task.Cancel();
            }

            _tasks.Clear();
            Dispose(true);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;    
            }
      
            if (disposing) {
                
                if (!_tasks.IsEmpty)
                {
                    Close();
                }
            }
      
            _disposed = true;
        }

        private SchedulerTask<T> CreateAndStore<T>(
            IScheduled<T> scheduled,
            T data,
            TimeSpan delayBefore,
            TimeSpan interval,
            bool repeats)
        {
            var task = new SchedulerTask<T>(scheduled, data, delayBefore, interval, repeats);
            _tasks.Push(task);
            return task;
        }

        private class SchedulerTask<T> : ICancellable, IRunnable
        {
            private readonly IScheduled<T> _scheduled;
            private readonly T _data;
            private readonly bool _repeats;
            private readonly TimeSpan _interval;
            private Timer? _timer;
            private bool _hasRun;
            private bool _isDisposed;

            public SchedulerTask(IScheduled<T> scheduled, T data, TimeSpan delayBefore, TimeSpan interval, bool repeats)
            {
                _scheduled = scheduled;
                _data = data;
                _repeats = repeats;
                _interval = interval;
                _hasRun = false;
                _isDisposed = false;
                // for scheduled in intervals and scheduled once we fire only the first time.
                _timer = new Timer(Tick, null, delayBefore, TimeSpan.FromMilliseconds(Timeout.Infinite));
            }

            private void Tick(object state) => Run();

            public void Run()
            {
                var start = DateTime.UtcNow;

                try
                {
                    _hasRun = true;
                    _scheduled.IntervalSignal(_scheduled, _data);

                    if (!_repeats)
                    {
                        Cancel();
                    }
                }
                finally
                {
                    // if we want to fire periodically we use timer.Change firing the next interval instead of relying
                    // on built in periodic callbacks of timer. Why ? Because this ensure that we will have just a single
                    // thread in the callback, where with the standard behaviour the callback has to be reentrant meaning
                    // that if enqueuing of the message is slow we can have several competing threads inside.
                    // this methods allows single thread in callback without locking.
                    // from : https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer?view=netcore-3.0
                    // The callback method executed by the timer should be reentrant, because it is called on ThreadPool threads.
                    // The callback can be executed simultaneously on two thread pool threads
                    // if the timer interval is less than the time required to execute the callback,
                    // or if all thread pool threads are in use and the callback is queued multiple times.
                    if (_interval.TotalMilliseconds >  0)
                    {
                        var elapsed = DateTime.UtcNow - start;
                        var dueIn = (int) (_interval - elapsed).TotalMilliseconds;
                        if (dueIn >= 0)
                        {
                            if (!_isDisposed)
                            {
                                _timer?.Change(TimeSpan.FromMilliseconds(dueIn), TimeSpan.FromMilliseconds(Timeout.Infinite));
                            }
                        }
                    }
                }
            }

            public bool Cancel()
            {
                if (_timer != null)
                {
                    _isDisposed = true;
                    _timer.Dispose();
                    _timer = null;
                }

                return _repeats || !_hasRun;
            }
        }
    }
}
