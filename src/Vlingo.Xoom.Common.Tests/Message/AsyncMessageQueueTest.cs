// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Common.Version;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Message
{
    public class AsyncMessageQueueTest: IDisposable
    {
        private readonly CountingDeadLettersQueue _countingDeadLettersQueue;
        private readonly CountingDeadLettersListener _countingDeadLettersListener;

        private readonly List<IMessage> _deliveredMessages;
        private readonly AsyncMessageQueue _queue;
        private readonly AsyncMessageQueue _exceptionsQueue;

        public AsyncMessageQueueTest()
        {
            _deliveredMessages = new List<IMessage>();

            _queue = new AsyncMessageQueue();
            _queue.RegisterListener(new ExceptionThrowingListener(false, _deliveredMessages));

            _countingDeadLettersListener = new CountingDeadLettersListener();
            _countingDeadLettersQueue = new CountingDeadLettersQueue();
            _countingDeadLettersQueue.RegisterListener(_countingDeadLettersListener);

            _exceptionsQueue = new AsyncMessageQueue(_countingDeadLettersQueue);
            _exceptionsQueue.RegisterListener(new ExceptionThrowingListener(true, _deliveredMessages));
        }

        [Fact]
        public void TestEnqueue()
        {
            _queue.Enqueue(new EmptyMessage());
            _queue.Enqueue(new EmptyMessage());
            _queue.Enqueue(new EmptyMessage());

            while (!_queue.IsEmpty) ;

            Assert.Equal(3, _deliveredMessages.Count);
        }

        [Fact]
        public void TestFlush()
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                _queue.Enqueue(new EmptyMessage());
            }

            _queue.Flush();

            Assert.Equal(1000, _deliveredMessages.Count);
        }

        [Fact]
        public void TestIsEmptyWithFlush()
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                _queue.Enqueue(new EmptyMessage());
            }

            Assert.False(_queue.IsEmpty);
            _queue.Flush();
            Assert.True(_queue.IsEmpty);
        }

        [Fact]
        public void TestClose()
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                _queue.Enqueue(new EmptyMessage());
            }

            _queue.Close();

            _queue.Enqueue(new EmptyMessage());

            _queue.Flush();

            Assert.NotEqual(1001, _deliveredMessages.Count);
            Assert.Equal(1000, _deliveredMessages.Count);
        }
        
        [Fact]
        public void TestDispose()
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                _queue.Enqueue(new EmptyMessage());
            }

            _queue.Dispose();

            _queue.Enqueue(new EmptyMessage());

            _queue.Flush();

            Assert.NotEqual(1001, _deliveredMessages.Count);
            Assert.Equal(1000, _deliveredMessages.Count);
        }

        [Fact]
        public void TestDeadLettersQueue()
        {
            var expected = 5;

            for (int idx = 0; idx < expected; ++idx)
            {
                _exceptionsQueue.Enqueue(new EmptyMessage());
            }

            _exceptionsQueue.Close();

            while (_countingDeadLettersQueue.HasNotCompleted(expected) ||
                    _countingDeadLettersListener.HasNotCompleted(expected))
            {
                Thread.Sleep(5);
            }

            Assert.Equal(5, _countingDeadLettersQueue.EnqueuedCount);
            Assert.Equal(5, _countingDeadLettersListener.HandledCount);
        }
        
        public void Dispose()
        {
            _countingDeadLettersQueue?.Dispose();
            _queue?.Dispose();
            _exceptionsQueue?.Dispose();
        }

        private class EmptyMessage : IMessage
        {
            public string Id { get; }
            public DateTimeOffset OccurredOn { get; }
            public T Payload<T>()
            {
                return default;
            }

            public string Type { get; }
            public string Version { get; }
            
            public SemanticVersion SemanticVersion { get; }
        }

        private class CountingDeadLettersListener : IMessageQueueListener
        {
            private readonly AtomicInteger _handledCount = new AtomicInteger(0);

            internal int HandledCount => _handledCount.Get();

            internal bool HasNotCompleted(int expected) => HandledCount < expected;

            public void HandleMessage(IMessage message) => _handledCount.GetAndIncrement();
        }

        private class ExceptionThrowingListener : IMessageQueueListener
        {
            private readonly bool _throwException;
            private readonly List<IMessage> _deliveredMessages;

            internal ExceptionThrowingListener(bool throwException, List<IMessage> deliveredMessages)
            {
                _throwException = throwException;
                _deliveredMessages = deliveredMessages;
            }

            public void HandleMessage(IMessage message)
            {
                if (_throwException)
                {
                    throw new Exception("test");
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    _deliveredMessages.Add(message);
                }
            }
        }

        private class CountingDeadLettersQueue : AsyncMessageQueue
        {
            private readonly AtomicInteger _enqueuedCount = new AtomicInteger(0);

            internal int EnqueuedCount => _enqueuedCount.Get();

            internal bool HasNotCompleted(int expected) => EnqueuedCount < expected;

            public override void Enqueue(IMessage message)
            {
                _enqueuedCount.GetAndIncrement();
                base.Enqueue(message);
            }
        }
    }
}
