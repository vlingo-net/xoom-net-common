// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
    public class Scheduler: IDisposable
    {
        private bool disposed;
        private readonly ConcurrentStack<SchedulerTask> tasks;

        public virtual ICancellable Schedule(IScheduled scheduled, object data, long delayBefore, long interval)
            => CreateAndStore(
                scheduled,
                data,
                TimeSpan.FromMilliseconds(delayBefore),
                TimeSpan.FromMilliseconds(interval),
                true);

        public virtual ICancellable ScheduleOnce(IScheduled scheduled, object data, long delayBefore, long interval)
            => CreateAndStore(
                scheduled,
                data,
                TimeSpan.FromMilliseconds(delayBefore + interval),
                TimeSpan.FromMilliseconds(Timeout.Infinite),
                false);


        public Scheduler()
        {
            tasks = new ConcurrentStack<SchedulerTask>();
        }

        public virtual void Close()
        {
            foreach(var task in tasks)
            {
                task.Cancel();
            }

            tasks.Clear();
            Dispose(true);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return; 
      
            if (disposing) {
                
                if (!tasks.IsEmpty)
                {
                    Close();
                }
            }
      
            disposed = true;
        }

        private SchedulerTask CreateAndStore(
            IScheduled scheduled,
            object data,
            TimeSpan delayBefore,
            TimeSpan interval,
            bool repeats)
        {
            var task = new SchedulerTask(scheduled, data, delayBefore, interval, repeats);
            tasks.Push(task);
            return task;
        }

        private class SchedulerTask : ICancellable
        {
            private readonly IScheduled scheduled;
            private readonly bool repeats;
            private Timer timer;
            private bool hasRun;

            public SchedulerTask(IScheduled scheduled, object data, TimeSpan delayBefore, TimeSpan interval, bool repeats)
            {
                this.scheduled = scheduled;
                this.repeats = repeats;
                hasRun = false;
                timer = new Timer(Tick, data, delayBefore, interval);
            }

            private void Tick(object data)
            {
                hasRun = true;
                scheduled.IntervalSignal(scheduled, data);

                if (!repeats)
                {
                    Cancel();
                }
            }

            public bool Cancel()
            {
                if (timer != null)
                {
                    using (timer)
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    timer = null;
                }

                return repeats || !hasRun;
            }
        }
    }
}
