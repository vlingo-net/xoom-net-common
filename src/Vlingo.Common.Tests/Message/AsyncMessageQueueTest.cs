// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using Vlingo.Common.Message;
using Xunit;

namespace Vlingo.Common.Tests.Message
{
    public class AsyncMessageQueueTest
    {
        private readonly CountingDeadLettersQueue countingDeadLettersQueue;
        private readonly CountingDeadLettersListener countingDeadLettersListener;

        private readonly List<IMessage> deliveredMessages;
        private readonly AsyncMessageQueue queue;
        private readonly AsyncMessageQueue exceptionsQueue;

        public AsyncMessageQueueTest()
        {
            deliveredMessages = new List<IMessage>();

            queue = new AsyncMessageQueue();
            queue.RegisterListener(new ExceptionThrowingListener(false, deliveredMessages));

            countingDeadLettersListener = new CountingDeadLettersListener();
            countingDeadLettersQueue = new CountingDeadLettersQueue();
            countingDeadLettersQueue.RegisterListener(countingDeadLettersListener);

            exceptionsQueue = new AsyncMessageQueue(countingDeadLettersQueue);
            exceptionsQueue.RegisterListener(new ExceptionThrowingListener(true, deliveredMessages));
        }

        [Fact]
        public void TestEnqueue()
        {
            queue.Enqueue(new Message());
            queue.Enqueue(new Message());
            queue.Enqueue(new Message());

            while (!queue.IsEmpty) ;

            Assert.Equal(3, deliveredMessages.Count);
        }

        [Fact]
        public void TestFlush()
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                queue.Enqueue(new Message());
            }

            queue.Flush();

            Assert.Equal(1000, deliveredMessages.Count);
        }

        [Fact]
        public void TestIsEmptyWithFlush()
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                queue.Enqueue(new Message());
            }

            Assert.False(queue.IsEmpty);
            queue.Flush();
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void TestClose()
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                queue.Enqueue(new Message());
            }

            queue.Close();

            queue.Enqueue(new Message());

            queue.Flush();

            Assert.NotEqual(1001, deliveredMessages.Count);
            Assert.Equal(1000, deliveredMessages.Count);
        }

        [Fact]
        public void TestDeadLettersQueue()
        {
            var expected = 5;

            for (int idx = 0; idx < expected; ++idx)
            {
                exceptionsQueue.Enqueue(new Message());
            }

            exceptionsQueue.Close();

            while (countingDeadLettersQueue.HasNotCompleted(expected) ||
                    countingDeadLettersListener.HasNotCompleted(expected))
            {
                Thread.Sleep(5);
            }

            Assert.Equal(5, countingDeadLettersQueue.EnqueuedCount);
            Assert.Equal(5, countingDeadLettersListener.HandledCount);
        }

        private class Message : IMessage { }

        private class CountingDeadLettersListener : IMessageQueueListener
        {
            private readonly AtomicInteger handledCount = new AtomicInteger(0);

            internal int HandledCount => handledCount.Get();

            internal bool HasNotCompleted(int expected) => HandledCount < expected;

            public void HandleMessage(IMessage message) => handledCount.GetAndIncrement();
        }

        private class ExceptionThrowingListener : IMessageQueueListener
        {
            private readonly bool throwException;
            private readonly List<IMessage> deliveredMessages;

            internal ExceptionThrowingListener(bool throwException, List<IMessage> deliveredMessages)
            {
                this.throwException = throwException;
                this.deliveredMessages = deliveredMessages;
            }

            public void HandleMessage(IMessage message)
            {
                if (throwException)
                {
                    throw new System.Exception("test");
                }
                else
                {
                    Thread.Sleep(System.TimeSpan.FromMilliseconds(1));
                    deliveredMessages.Add(message);
                }
            }
        }

        private class CountingDeadLettersQueue : AsyncMessageQueue
        {
            private readonly AtomicInteger enqueuedCount = new AtomicInteger(0);

            internal int EnqueuedCount => enqueuedCount.Get();

            internal bool HasNotCompleted(int expected) => EnqueuedCount < expected;

            public override void Enqueue(IMessage message)
            {
                enqueuedCount.GetAndIncrement();
                base.Enqueue(message);
            }
        }
    }
}
