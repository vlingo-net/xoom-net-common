// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Xunit;

namespace Vlingo.Common.Tests
{
    public class BasicCompletesStateStateTest
    {
        private Scheduler _testScheduler;
        
        [Fact]
        public void TestCompletesWith()
        {
            var completes = new BasicCompletesState<int>(5);

            Assert.Equal(5, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterFunction()
        {
            var completes = new BasicCompletesState<int>(0);
            completes.AndThen(value => value * 2);

            completes.With(5);

            Assert.Equal(10, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterConsumer()
        {
            var andThenValue = 0;
            var completes = new BasicCompletesState<int>(0);
            completes.AndThen(x => andThenValue = x);

            completes.With(5);

            var completed = completes.Await<int>();
            
            Assert.Equal(5, andThenValue);
            Assert.Equal(5, completed);
        }

        [Fact]
        public void TestCompletesAfterAndThen()
        {
            var andThenValue = 0;
            var completes = new BasicCompletesState<int>(0);
            completes
                .AndThen(value => value * 2)
                .AndThen(x => andThenValue = x);

            completes.With(5);

            var completed = completes.Await<int>();

            Assert.Equal(10, andThenValue);
            Assert.Equal(10, completed);
        }
        
//        [Fact]
//        public void TestCompletesAfterAndThenAndThen()
//        {
//            var andThenValue = string.Empty;
//            var completes = new BasicCompletesState<int>(0);
//            completes
//                .AndThen(value => value * 2)
//                .AndThen(value => value.ToString())
//                .AndThen(x => andThenValue = x);
//
//            completes.With(5);
//
//            Assert.Equal("10", andThenValue);
//        }
//
//        [Fact]
//        public void TestCompletesAfterAndThenMessageOut()
//        {
//            var andThenValue = 0;
//            var completes = new BasicCompletesState<int>(0);
//            var sender = new Sender(x => andThenValue = x);
//
//            completes
//                .AndThen(value => value * 2)
//                .AndThen(x => { sender.Send(x); return x; });
//
//            completes.With(5);
//
//            Assert.Equal(10, andThenValue);
//        }
//
//        [Fact]
//        public void TestOutcomeBeforeTimeout()
//        {
//            var andThenValue = 0;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(TimeSpan.FromMilliseconds(1000), value => value * 2)
//                .AndThen(x => andThenValue = x);
//
//            completes.With(5);
//            var completed = completes.Await(TimeSpan.FromMilliseconds(10));
//
//            Assert.Equal(10, andThenValue);
//            Assert.Equal(10, completed);
//        }
//
//        [Fact]
//        public void TestTimeoutBeforeOutcome()
//        {
//            var andThenValue = 0;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(TimeSpan.FromMilliseconds(1), -10, value => value * 2)
//                .AndThen(x => andThenValue = x);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(10);
//                completes.With(5);
//            });
//            thread.Start();
//
//            var completed = completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.NotEqual(10, andThenValue);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(-10, completed);
//        }
//
//        [Fact]
//        public void TestThatFailureOutcomeFails()
//        {
//            int andThenValue = -1, failureValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            completes
//                .AndThen(-100, value => 2 * value)
//                .AndThen(x => andThenValue = x)
//                .Otherwise<int>(x => failureValue = 1000);
//
//            completes.With(-100);
//            var completed = completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(-1, andThenValue);
//            Assert.Equal(1000, failureValue);
//            Assert.Equal(1000, completed);
//        }
//        
//        [Fact]
//        public void TestThatFailureOutcomeFailsWhenScheduled()
//        {
//            var andThenValue = 0;
//            var failedValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(TimeSpan.FromMilliseconds(200), -10, value => value * 2)
//                .AndThen(x => andThenValue = 100)
//                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(-10);
//            });
//            thread.Start();
//
//            var completed = completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(-10, failedValue);
//            Assert.Equal(-10, completed);
//        }
//        
//        [Fact]
//        public void TestThatFailureOutcomeFailsWhenScheduledTimesOut()
//        {
//            var andThenValue = 0;
//            var failedValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(TimeSpan.FromMilliseconds(1), -10, value => value * 2)
//                .AndThen(x => andThenValue = 100)
//                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(5);
//            });
//            thread.Start();
//
//            completes.Await();
//            
//            Assert.True(completes.HasFailed);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(-10, failedValue);
//        }
//        
//        [Fact]
//        public void TestThatFailureOutcomeFailsWhenScheduledTimesOutWithOneAndThen()
//        {
//            var andThenValue = 0;
//            var failedValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(TimeSpan.FromMilliseconds(1), -10, value => value * 2)
//                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(10);
//                completes.With(5);
//            });
//            thread.Start();
//
//            completes.Await();
//            
//            Assert.True(completes.HasFailed);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(-10, failedValue);
//        }
//        
//        [Fact]
//        public void TestThatFailureOutcomeFailsWhenScheduledInMiddle()
//        {
//            var andThenValue = 0;
//            var failedValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(x => x * x)
//                .AndThen(TimeSpan.FromMilliseconds(200), 100, value => andThenValue = value * 2)
//                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(10);
//            });
//            thread.Start();
//
//            completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(100, failedValue);
//        }
//        
//        [Fact]
//        public void TestThatFailureOutcomeFailsInMiddle()
//        {
//            int andThenValue = -1, failureValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            completes
//                .AndThen(value => value * value)
//                .AndThen(100, x => andThenValue = 200)
//                .Otherwise<int>(x => failureValue = 1000);
//
//            completes.With(10);
//            completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(-1, andThenValue);
//            Assert.Equal(1000, failureValue);
//        }
//        
//        [Fact]
//        public void TestThatFailureOutcomeFailsInMiddleWithChangedType()
//        {
//            var andThenValue = string.Empty;
//            var failureValue = string.Empty;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            completes
//                .AndThen(value => (value * value).ToString())
//                .AndThen("100", x => andThenValue = "200")
//                .Otherwise<string>(x => failureValue = "1000");
//
//            completes.With(10);
//            completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(string.Empty, andThenValue);
//            Assert.Equal("1000", failureValue);
//        }
//        
//        [Fact]
//        public void TestThatExceptionOutcomeInvalidCast()
//        {
//            var completes = new BasicCompletesState<string>(_testScheduler);
//            completes
//                .AndThen("-100", value => (2 * int.Parse(value)).ToString())
//                .AndThen(x => int.Parse(x))
//                .Otherwise<int>(x => 1000);
//
//            Assert.Throws<InvalidCastException>(() => completes.With("-100"));
//        }
//
//        [Fact]
//        public void TestThatExceptionOutcomeFails()
//        {
//            var failureValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(42, value => value * 2)
//                .AndThen<int>(value => throw new ApplicationException((2 * value).ToString()))
//                .RecoverFrom(e => failureValue = int.Parse(e.Message));
//
//            completes.With(2);
//            completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(8, failureValue);
//        }
//        
//        [Fact]
//        public void TestThatExceptionOtherwiseFails()
//        {
//            var failureValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThen(42, value => value * 2)
//                .AndThen<int>(value => throw new ApplicationException((2 * value).ToString()))
//                .Otherwise<int>(v => throw new ApplicationException(v.ToString()))
//                .RecoverFrom(e => failureValue = int.Parse(e.Message));
//
//            completes.With(42);
//            completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(42, failureValue);
//        }
//
//        [Fact]
//        public void TestThatExceptionHandlerDelayRecovers()
//        {
//            var failureValue = -1;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            completes
//                .AndThen(0, value => value * 2)
//                .AndThen<int>(value => throw new Exception($"{value * 2}"));
//
//            completes.With(10);
//
//            completes.RecoverFrom(e => failureValue = int.Parse(e.Message));
//
//            completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(40, failureValue);
//        }
//
//        [Fact]
//        public void TestThatAwaitTimesOut()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            var completed = completes.Await(TimeSpan.FromMilliseconds(10));
//
//            completes.With(5);
//
//            Assert.NotEqual(5, completed);
//            Assert.Equal(default, completed);
//        }
//
//        [Fact]
//        public void TestThatAwaitCompletes()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(5);
//            });
//            thread.Start();
//
//            var completed = completes.Await();
//
//            Assert.Equal(5, completed);
//        }
//
//        [Fact]
//        public void TestAndThenToCompletesCastingOutput()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            completes.AndThenTo(v => (v * 10).ToString());
//            completes.With(10);
//            var result = completes.Await();
//
//            Assert.Equal(100, result);
//        }
//        
//        [Fact]
//        public void TestAndThenToCompletes()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            completes.AndThenTo(v => (v * 10).ToString());
//            completes.With(10);
//            var result = completes.Await<string>();
//
//            Assert.Equal("100", result);
//        }
        
//        [Fact]
//        public void TestAndThenToFails()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            completes.AndThenTo(10, v => v * 10);
//            completes.With(10);
//            var result = completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(10, result);
//        }
//        
//        [Fact]
//        public void TestAndThenToFailsWhenScheduledTimesOut()
//        {
//            var completes = new BasicCompletesState<int>(new TestScheduler());
//            completes.AndThenTo(TimeSpan.FromMilliseconds(1), 10, v => v * 10);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(5);
//            });
//            thread.Start();
//
//            var result = completes.Await();
//            
//            Assert.True(completes.HasFailed);
//            Assert.Equal(10, result);
//        }
        
//        [Fact]
//        public void TestAndThenToOutcomeBeforeTimeout()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes.AndThenTo(TimeSpan.FromMilliseconds(1000), value => value * 2);
//
//            completes.With(5);
//            var result = completes.Await(TimeSpan.FromMilliseconds(10));
//
//            Assert.Equal(10, result);
//        }
//        
//        [Fact]
//        public void TestAndThenToOutcomeBeforeTimeoutWithResult()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes.AndThenTo(TimeSpan.FromMilliseconds(1000), value => (value * 2).ToString());
//
//            completes.With(5);
//            
//            var result = completes.Await<string>(TimeSpan.FromMilliseconds(10));
//
//            Assert.Equal("10", result);
//        }
//        
//        [Fact]
//        public void TestOtherwiseConsume()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            var failedResult = -1;
//            
//            completes
//                .AndThenTo(5, v => Completes.WithSuccess(v * 2))
//                .OtherwiseConsume(failedValue => failedResult = failedValue);
//            
//            completes.With(5);
//
//            completes.Await();
//            
//            Assert.Equal(5, failedResult);
//        }
//        
//        [Fact]
//        public void TestOtherwiseConsumeAfterTimeout()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            var failedResult = -1;
//
//            completes
//                .AndThenTo(TimeSpan.FromMilliseconds(1), 5, v => Completes.WithSuccess(v * 2))
//                .OtherwiseConsume(failedValue => failedResult = failedValue);
//            
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(10);
//                completes.With(10);
//            });
//            thread.Start();
//
//            completes.Await();
//            
//            Assert.Equal(5, failedResult);
//        }
//        
//        [Fact]
//        public void TestConsumeBeforeTimeout()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThenTo(TimeSpan.FromMilliseconds(1000), v => Completes.WithSuccess(v * 2));
//            
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(10);
//            });
//            thread.Start();
//            
//            var completed = completes.Await();
//            
//            Assert.Equal(20, completed);
//        }
//        
//        [Fact]
//        public void TestAndThenConsumeCalled()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            var consumedResult = -1;
//            
//            completes
//                .AndThenTo(v => Completes.WithSuccess(v * 2))
//                .AndThenConsume(v => consumedResult = v);
//            
//            completes.With(5);
//
//            var completed = completes.Await();
//            
//            Assert.Equal(10, consumedResult);
//            Assert.Equal(10, completed);
//        }
//        
//        [Fact]
//        public void TestAndThenConsumeBeforeTimeout()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            var consumedResult = -1;
//            
//            completes
//                .AndThenTo(v => Completes.WithSuccess(v * 2))
//                .AndThenConsume(TimeSpan.FromMilliseconds(1000), v => consumedResult = v);
//            
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(5);
//            });
//            thread.Start();
//            
//            var completed = completes.Await();
//            
//            Assert.Equal(10, consumedResult);
//            Assert.Equal(10, completed);
//        }
//        
//        [Fact]
//        public void TestAndThenConsumeNotRunAfterTimeout()
//        {
//            var scheduler = _testScheduler;
//            var completes = new BasicCompletesState<int>(scheduler);
//            var consumedResult = -1;
//
//            completes
//                .AndThenTo(v => Completes.Using<int>(scheduler))
//                .AndThenConsume(TimeSpan.FromMilliseconds(1), v => consumedResult = v);
//            
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(10);
//                completes.With(5);
//            });
//            thread.Start();
//
//            var completed = completes.Await();
//            
//            Assert.Equal(-1, consumedResult);
//            Assert.Equal(0, completed);
//        }
//
//        [Fact]
//        public void TestThatTimeOutOccursForSideEffects()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            var consumedResult = -1;
//
//            completes
//                .AndThenConsume(TimeSpan.FromMilliseconds(2), 0, value => consumedResult = value)
//                .Otherwise<int>(value =>
//                {
//                    consumedResult = value;
//                    return 0;
//                });
//            
//            
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(10);
//                completes.With(1);
//            });
//            thread.Start();
//
//            completes.Await();
//            
//            Assert.NotEqual(1, consumedResult);
//            Assert.Equal(0, consumedResult);
//            Assert.Equal(0, completes.Outcome);
//        }
//        
//        [Fact]
//        public void TestAndThenConsumeFails()
//        {
//            var completes = new BasicCompletesState<int>(_testScheduler);
//            var consumedResult = -1;
//            
//            completes
//                .AndThenTo(v => Completes.WithSuccess(v * 2))
//                .AndThenConsume(10, v => consumedResult = v);
//            
//            completes.With(5);
//
//            var completed = completes.Await();
//            
//            Assert.True(completes.HasFailed);
//            Assert.Equal(-1, consumedResult);
//            Assert.Equal(10, completed);
//        }
//        
//        [Fact]
//        public void TestAndThenConsumeTimeoutBeforeOutcome()
//        {
//            var andThenValue = 0;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThenTo(v => Completes.WithSuccess(v * 2))
//                .AndThenConsume(TimeSpan.FromMilliseconds(1), 10, x => andThenValue = x);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(10);
//                completes.With(5);
//            });
//            thread.Start();
//
//            var completed = completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(10, completed);
//        }
//        
//        [Fact]
//        public void TestAndThenConsumeOutcomesBeforeTiemout()
//        {
//            var andThenValue = 0;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThenTo(v => Completes.WithSuccess(v * 2))
//                .AndThenConsume(TimeSpan.FromMilliseconds(1000), -10, x => andThenValue = x);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(5);
//            });
//            thread.Start();
//
//            var completed = completes.Await();
//
//            Assert.False(completes.HasFailed);
//            Assert.Equal(10, andThenValue);
//            Assert.Equal(10, completed);
//        }
//
//        [Fact]
//        public void TestAndThenConsumeFailsBeforeTimeoutWithFailedOutcome()
//        {
//            var andThenValue = 0;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThenTo(v => Completes.WithSuccess(v * 2))
//                .AndThenConsume(TimeSpan.FromMilliseconds(1000), 10, x => andThenValue = x);
//
//            var thread = new Thread(() =>
//            {
//                Thread.Sleep(100);
//                completes.With(5);
//            });
//            thread.Start();
//
//            var completed = completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(10, completed);
//        }
//        
//        [Fact]
//        public void TestOtherwiseConsumeIsHandled()
//        {
//            var andThenValue = 0;
//            var failedValue = 0;
//            var completes = new BasicCompletesState<int>(_testScheduler);
//
//            completes
//                .AndThenTo(v => Completes.WithSuccess(v * 2))
//                .AndThenConsume(10, x => andThenValue = x)
//                .OtherwiseConsume(v => failedValue = v);
//            
//            Thread.Sleep(100);
//            completes.With(5);
//
//            completes.Await();
//
//            Assert.True(completes.HasFailed);
//            Assert.Equal(0, andThenValue);
//            Assert.Equal(10, failedValue);
//        }
//        
//        [Fact]
//        public void TestAndThenToWithComplexType()
//        {
//            var completes = new BasicCompletesState<IUser>(_testScheduler);
//
//            completes.AndThenTo(user => user.WithName("Tomasz"));
//
//            completes.With<IUser>(new User());
//
//            var completed = completes.Await<UserState>();
//            
//            Assert.Equal("Tomasz", completed.Name);
//        }
//        
//        [Fact]
//        public void TestAndThenToWithComplexTypes()
//        {
//            var completes = new BasicCompletesState<IUser>(_testScheduler);
//            UserState expectedUserState = null;
//            completes
//                .AndThenTo(user => user.WithName("Tomasz"))
//                .OtherwiseConsume(noUser => Completes.WithSuccess(new UserState(string.Empty, string.Empty, string.Empty)))
//                .AndThenConsume(userState => {
//                    expectedUserState = userState;
//                });
//
//            completes.With<IUser>(new User());
//
//            completes.Await();
//            
//            Assert.Equal("Tomasz", expectedUserState?.Name);
//        }
//        
//        [Fact]
//        public void TestAwaitWithGenericTypes()
//        {
//            var readerActor = new InMemoryStateStoreEntryReaderActor<string>();
//            var completes = new BasicCompletesState<IStateStoreEntryReader<string>>(readerActor);
//            var completed = completes.Await();
//            Assert.Equal(readerActor, completed);
//        }

        public BasicCompletesStateStateTest()
        {
            _testScheduler = new TestScheduler();
        }

        private class Sender
        {
            private readonly Action<int> callback;
            public Sender(Action<int> callback)
            {
                if (callback != null)
                {
                    this.callback = callback;
                }
            }

            internal void Send(int value)
            {
                callback(value);
            }
        }

        private interface IUser
        {
            ICompletes<UserState> WithContact(string contact);
            ICompletes<UserState> WithName(string name);
        }

        private class User : IUser
        {
            private UserState _userState;

            public string Name => _userState.Name;

            public User()
            {
                _userState = new UserState("1", "1", "1");
            }
        
            public ICompletes<UserState> WithContact(string contact)
            {
                return Completes.WithSuccess(_userState.WithContact(contact));
            }

            public ICompletes<UserState> WithName(string name)
            {
                return Completes.WithSuccess(_userState.WithName(name));
            }
        }

        private class UserState
        {
            public string Id { get; }
            public string Name { get; }
            public string Contact { get; }

            public UserState WithName(string name) => new UserState(Id, name, Contact);
        
            public UserState WithContact(string contact) => new UserState(Id, Name, contact);

            public UserState(string id, string name, string contact)
            {
                Id = id;
                Name = name;
                Contact = contact;
            }
        }

        private interface IStateStoreEntryReader<TEntry>
        {
        }

        private class InMemoryStateStoreEntryReaderActor<TEntry> : IStateStoreEntryReader<TEntry>
        {
        }
    }
}
