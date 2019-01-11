// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Vlingo.Common.Message
{
    public class AsyncMessageQueue : IMessageQueue, IRunnable
    {
        private readonly IMessageQueue deadLettersQueue;
        private readonly AtomicBoolean dispatching;
        private IMessageQueueListener listener;
        private readonly AtomicBoolean open;
        private readonly ConcurrentQueue<IMessage> queue;

        private readonly CancellationTokenSource cancellationSource;
        private readonly AutoResetEvent resetEvent;

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

            resetEvent = new AutoResetEvent(false);
            cancellationSource = new CancellationTokenSource();
            Task.Run(() => TaskAction(), cancellationSource.Token);
        }

        public virtual void Close() => Close(true);

        public virtual void Close(bool flush)
        {
            if (open.Get())
            {
                open.Set(false);

                if (flush)
                {
                    Flush();
                }

                cancellationSource.Cancel();
                resetEvent.Set();
            }
        }

        public virtual void Enqueue(IMessage message)
        {
            if (open.Get())
            {
                queue.Enqueue(message);
                resetEvent.Set();
            }
        }

        public virtual void Flush()
        {
            while (!queue.IsEmpty)
            {
                resetEvent.WaitOne();
            }

            while (dispatching.Get())
            {
                resetEvent.WaitOne();
            }
        }

        public virtual bool IsEmpty => queue.IsEmpty && !dispatching.Get();

        public virtual void RegisterListener(IMessageQueueListener listener)
        {
            open.Set(true);
            this.listener = listener;
        }

        public virtual void Run()
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
                resetEvent.Set();
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

        private void TaskAction()
        {
            while (!cancellationSource.IsCancellationRequested)
            {
                resetEvent.WaitOne();
                while (!queue.IsEmpty)
                {
                    Run();
                }
            }
        }
    }
}
