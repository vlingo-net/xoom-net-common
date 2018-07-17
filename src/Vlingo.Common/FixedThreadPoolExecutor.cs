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
    public class FixedThreadPoolExecutor
    {
        private readonly int poolSize;
        private readonly ConcurrentQueue<IRunnable> queue;
        private int runningThreadCount;
        private readonly Thread watcherThread;

        private volatile bool shouldWatcherRun = true;

        public FixedThreadPoolExecutor(int poolSize)
        {
            this.poolSize = poolSize;
            queue = new ConcurrentQueue<IRunnable>();
            runningThreadCount = 0;
            watcherThread = new Thread(WatcherThreadRun);
            watcherThread.Start();
        }

        public void ShutdownNow()
        {
            shouldWatcherRun = false;
            watcherThread.Interrupt();
            queue.Clear();
        }

        public void Execute(IRunnable command)
        {
            queue.Enqueue(command);
            watcherThread.Interrupt();
        }

        private void WatcherThreadRun()
        {
            while (shouldWatcherRun)
            {
                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException)
                {
                    // do nothing
                }
                catch (ThreadAbortException)
                {
                    // no need to panic. you are just shutting down...
                }

                StartExecutorThreads();
            }
        }

        private void StartExecutorThreads()
        {
            var currentlyRunningThreads = Interlocked.CompareExchange(ref runningThreadCount, 0, 0);
            if(currentlyRunningThreads < poolSize)
            {
                while (!queue.IsEmpty && TryIncrementRunningThread())
                {
                    if (queue.TryDequeue(out IRunnable task))
                    {
                        var thread = new Thread(new ParameterizedThreadStart(ExecutorThreadRun));
                        thread.Start(task);
                    }
                    else
                    {
                        Interlocked.Decrement(ref runningThreadCount);
                    }
                }
            }
        }

        private bool TryIncrementRunningThread()
        {
            var currentValue = Interlocked.CompareExchange(ref runningThreadCount, 0, 0);
            while(currentValue < poolSize)
            {
                var valueNow = Interlocked.CompareExchange(ref runningThreadCount, currentValue + 1, currentValue);
                if(valueNow == currentValue)
                {
                    return true;
                }
                currentValue = Interlocked.CompareExchange(ref runningThreadCount, 0, 0);
            }
            return false;
        }

        private void ExecutorThreadRun(object task)
        {
            try
            {
                ((IRunnable)task).Run();
            }
            catch { }
            finally
            {
                Interlocked.Decrement(ref runningThreadCount);
                watcherThread.Interrupt();
            }
        }
    }
}
