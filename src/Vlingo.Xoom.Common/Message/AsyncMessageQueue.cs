// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Vlingo.Xoom.Common.Message
{
    public class AsyncMessageQueue : IMessageQueue, IRunnable, IDisposable
    {
        private bool _disposed;
        private readonly IMessageQueue? _deadLettersQueue;
        private readonly AtomicBoolean _dispatching;
        private IMessageQueueListener? _listener;
        private readonly AtomicBoolean _open;
        private readonly ConcurrentQueue<IMessage> _queue;

        private readonly CancellationTokenSource _cancellationSource;
        private readonly AutoResetEvent _taskProcessedEvent;
        private readonly AutoResetEvent _endDispatchingEvent;
        private readonly Task _backgroundWorker;

        public AsyncMessageQueue()
            : this(null)
        {
        }

        public AsyncMessageQueue(IMessageQueue? deadLettersQueue)
        {
            _deadLettersQueue = deadLettersQueue;
            _dispatching = new AtomicBoolean(false);
            _open = new AtomicBoolean(false);
            _queue = new ConcurrentQueue<IMessage>();

            _taskProcessedEvent = new AutoResetEvent(false);
            _endDispatchingEvent = new AutoResetEvent(false);
            _cancellationSource = new CancellationTokenSource();
            _backgroundWorker = Task.Run(() => TaskAction(_cancellationSource.Token), _cancellationSource.Token);
        }

        public virtual void Close() => Close(true);

        public virtual void Close(bool flush)
        {
            if (_open.Get())
            {
                _open.Set(false);

                if (flush)
                {
                    Flush();
                }

                _cancellationSource.Cancel();
                _taskProcessedEvent.Set();
                try
                {
                    _backgroundWorker.Wait(_cancellationSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // TODO: log or do nothing but everything is ok
                }
            }
            Dispose(true);
        }

        public virtual void Enqueue(IMessage message)
        {
            if (_open.Get())
            {
                _queue.Enqueue(message);
                _taskProcessedEvent.Set();
            }
        }

        public virtual void Flush()
        {
            while (!_queue.IsEmpty)
            {
                _endDispatchingEvent.WaitOne();
            }

            while (_dispatching.Get())
            {
                _endDispatchingEvent.WaitOne();
            }
        }

        public virtual bool IsEmpty => _queue.IsEmpty && !_dispatching.Get();

        public virtual void RegisterListener(IMessageQueueListener listener)
        {
            _open.Set(true);
            _listener = listener;
        }

        public virtual void Run()
        {
            IMessage? message = null;

            try
            {
                _dispatching.Set(true);
                message = Dequeue();
                if (message != null)
                {
                    _listener?.HandleMessage(message);
                }
            }
            catch (Exception e)
            {
                // TODO: Log
                if (message != null && _deadLettersQueue != null)
                {
                    _deadLettersQueue.Enqueue(message);
                }
                Console.WriteLine($"AsyncMessageQueue: Dispatch to listener has failed because: {e.Message}");
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                _dispatching.Set(false);
                _endDispatchingEvent.Set();
            }
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
      
            if (disposing)
            {
                
                if (_open.Get())
                {
                    Close(true);
                }

                _cancellationSource.Dispose();
                _taskProcessedEvent.Dispose();
                _endDispatchingEvent.Dispose();
            }
      
            _disposed = true;
        }

        private IMessage? Dequeue()
        {
            if(_queue.TryDequeue(out IMessage? msg))
            {
                return msg;
            }

            return null;
        }

        private void TaskAction(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _taskProcessedEvent.WaitOne();
                while (!_queue.IsEmpty)
                {
                    Run();
                }
            }
        }
    }
}
