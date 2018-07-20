// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Vlingo.Common.Message
{
    public class AsyncMessageQueue : IMessageQueue, IRunnable
    {
        private readonly IMessageQueue deadLettersQueue;
        private AtomicBoolean dispatching;
        private IMessageQueueListener listener;
        private AtomicBoolean open;
        private readonly ConcurrentQueue<IMessage> queue;

        private readonly Thread executorThread;
        private volatile bool shouldExecutorRun = true;

        public AsyncMessageQueue()
            : this(null)
        {
        }

        public AsyncMessageQueue(IMessageQueue deadLettersQueue)
        {
            this.deadLettersQueue = deadLettersQueue;
            dispatching = new AtomicBoolean(false);
            open = new AtomicBoolean(false);
            queue = new ConcurrentQueue<IMessage>();

            executorThread = new Thread(ExecutorThreadStart);
            executorThread.Start();
        }

        public void Close() => Close(true);

        public void Close(bool flush)
        {
            if (open.Get())
            {
                open.Set(false);

                if (flush)
                {
                    Flush();
                }

                shouldExecutorRun = false;
                executorThread.Interrupt();
            }
        }

        public virtual void Enqueue(IMessage message)
        {
            if (open.Get())
            {
                queue.Enqueue(message);
                executorThread.Interrupt();
            }
        }

        public void Flush()
        {
            try
            {
                while (!queue.IsEmpty)
                {
                    Thread.Sleep(1);
                }

                while (dispatching.Get())
                {
                    Thread.Sleep(1);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public bool IsEmpty => queue.IsEmpty && !dispatching.Get();

        public void RegisterListener(IMessageQueueListener listener)
        {
            open.Set(true);
            this.listener = listener;
        }

        public void Run()
        {
            IMessage message = null;

            try
            {
                dispatching.Set(true);
                message = Dequeue();
                if (message != null)
                {
                    listener.HandleMessage(message);
                }
            }
            catch (Exception e)
            {
                // TODO: Log
                if (message != null && deadLettersQueue != null)
                {
                    deadLettersQueue.Enqueue(message);
                }
                Console.WriteLine($"AsyncMessageQueue: Dispatch to listener failed because: {e.Message}");
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                dispatching.Set(false);
            }
        }

        private IMessage Dequeue()
        {
            if(queue.TryDequeue(out IMessage msg))
            {
                return msg;
            }

            return null;
        }

        private void ExecutorThreadStart()
        {
            while (shouldExecutorRun)
            {
                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch(Exception) { }

                while (!queue.IsEmpty)
                {
                    Run();
                }
            }
        }
    }
}
