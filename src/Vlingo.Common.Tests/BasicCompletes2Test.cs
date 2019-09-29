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
    public class BasicCompletes2Test
    {
        [Fact]
        public void TestCompletesWith()
        {
            var completes = new BasicCompletes2<int>(5);

            Assert.Equal(5, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterFunction()
        {
            var completes = new BasicCompletes2<int>(0);
            completes.AndThen(value => value * 2);

            completes.With(5);

            Assert.Equal(10, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterConsumer()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes2<int>(0);
            completes.AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(5, andThenValue);
        }

        [Fact]
        public void TestCompletesAfterAndThen()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes2<int>(0);
            completes
                .AndThen(value => value * 2)
                .AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(10, andThenValue);
        }
        
        [Fact]
        public void TestCompletesAfterAndThenAndThen()
        {
            string andThenValue = string.Empty;
            var completes = new BasicCompletes2<int>(0);
            completes
                .AndThen(value => value * 2)
                .AndThen(value => value.ToString())
                .AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal("10", andThenValue);
        }

        [Fact]
        public void TestCompletesAfterAndThenMessageOut()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes2<int>(0);
            var sender = new Sender(x => andThenValue = x);

            completes
                .AndThen(value => value * 2)
                .AndThen(x => { sender.Send(x); return x; });

            completes.With(5);

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestOutcomeBeforeTimeout()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(1000), value => value * 2)
                .AndThen(x => andThenValue = x);

            completes.With(5);
            completes.Await(TimeSpan.FromMilliseconds(10));

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestTimeoutBeforeOutcome()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(1), -10, value => value * 2)
                .AndThen(x => andThenValue = x);

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();

            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.NotEqual(10, andThenValue);
            Assert.Equal(0, andThenValue);
        }

        [Fact]
        public void TestThatFailureOutcomeFails()
        {
            int andThenValue = -1, failureValue = 999;
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes
                .AndThen(-100, value => 2 * value)
                .AndThen(x => andThenValue = x)
                .Otherwise<int>(x => failureValue = 1000);

            completes.With(-100);
            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(-1, andThenValue);
            Assert.Equal(1000, failureValue);
        }
        
        [Fact]
        public void TestThatFailureOutcomeFailsWhenScheduled()
        {
            int andThenValue = 0;
            int failedValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(200), -10, value => value * 2)
                .AndThen(x => andThenValue = 100)
                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(-10);
            }));
            thread.Start();

            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(0, andThenValue);
            Assert.Equal(-10, failedValue);
        }
        
        [Fact]
        public void TestThatFailureOutcomeFailsWhenScheduledTimesOut()
        {
            int andThenValue = 0;
            int failedValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(1), -10, value => value * 2)
                .AndThen(x => andThenValue = 100)
                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();

            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(0, andThenValue);
            Assert.Equal(-10, failedValue);
        }
        
        [Fact]
        public void TestThatFailureOutcomeFailsWhenScheduledTimesOutWithOneAndThen()
        {
            int andThenValue = 0;
            int failedValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(1), -10, value => value * 2)
                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();

            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(0, andThenValue);
            Assert.Equal(-10, failedValue);
        }
        
        [Fact]
        public void TestThatFailureOutcomeFailsWhenScheduledInMiddle()
        {
            int andThenValue = 0;
            int failedValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(x => andThenValue = 100)
                .AndThen(TimeSpan.FromMilliseconds(200), -10, value => andThenValue = value * 2)
                .Otherwise<int>(failedOutcome => failedValue = failedOutcome);

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(-10);
            }));
            thread.Start();

            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(100, andThenValue);
            Assert.Equal(-10, failedValue);
        }
        
        [Fact]
        public void TestThatFailureOutcomeFailsInMiddle()
        {
            int andThenValue = -1, failureValue = 999;
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes
                .AndThen(value => andThenValue = 100)
                .AndThen(-100, x => andThenValue = 200)
                .Otherwise<int>(x => failureValue = 1000);

            completes.With(-100);
            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(100, andThenValue);
            Assert.Equal(1000, failureValue);
        }
        
        [Fact]
        public void TestThatExceptionOutcomeInvalidCast()
        {
            int andThenValue = -1, failureValue = 999;
            var completes = new BasicCompletes2<string>(new Scheduler());
            completes
                .AndThen("-100", value => (2 * int.Parse(value)).ToString())
                .AndThen(x => andThenValue = int.Parse(x))
                .Otherwise<int>(x => failureValue = 1000);

            Assert.Throws<InvalidCastException>(() => completes.With("-100"));
        }

        [Fact]
        public void TestThatExceptionOutcomeFails()
        {
            int failureValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(42, value => value * 2)
                .AndThen<int>(value => throw new ApplicationException((2 * value).ToString()))
                .RecoverFrom(e => failureValue = int.Parse(e.Message));

            completes.With(2);
            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(8, failureValue);
        }
        
        [Fact]
        public void TestThatExceptionOtherwiseFails()
        {
            int failureValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(42, value => value * 2)
                .AndThen<int>(value => throw new ApplicationException((2 * value).ToString()))
                .Otherwise<int>(v => throw new ApplicationException(v.ToString()))
                .RecoverFrom(e => failureValue = int.Parse(e.Message));

            completes.With(42);
            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(42, failureValue);
        }

        [Fact]
        public void TestThatExceptionHandlerDelayRecovers()
        {
            var failureValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes
                .AndThen(0, value => value * 2)
                .AndThen<int>(value => throw new Exception($"{value * 2}"));

            completes.With(10);

            completes.RecoverFrom(e => failureValue = int.Parse(e.Message));

            completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(40, failureValue);
        }

        [Fact]
        public void TestThatAwaitTimesOut()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());

            var completed = completes.Await(TimeSpan.FromMilliseconds(10));

            completes.With(5);

            Assert.NotEqual(5, completed);
            Assert.Equal(default, completed);
        }

        [Fact]
        public void TestThatAwaitCompletes()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();

            var completed = completes.Await();

            Assert.Equal(5, completed);
        }

        [Fact]
        public void TestAndThenToCompletes()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes.AndThenTo(v => (v * 10).ToString());
            completes.With(10);
            var result = completes.Await();

            Assert.Equal(10, result);
        }
        
        [Fact]
        public void TestAndThenToFails()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes.AndThenTo(10, v => v * 10);
            completes.With(10);
            var result = completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(10, result);
        }
        
        [Fact]
        public void TestAndThenToFailsWhenScheduledTimesOut()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes.AndThenTo(TimeSpan.FromMilliseconds(1), 10, v => v * 10);

            var result = -1;
            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();
            
            result = completes.Await();

            Assert.True(completes.HasFailed);
            Assert.Equal(10, result);
        }
        
        [Fact]
        public void TestAndThenToOutcomeBeforeTimeout()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes.AndThenTo(TimeSpan.FromMilliseconds(1000), value => value * 2);

            completes.With(5);
            var result = completes.Await(TimeSpan.FromMilliseconds(10));

            Assert.Equal(10, result);
        }
        
        [Fact]
        public void TestAndThenToOutcomeBeforeTimeoutWithResult()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes.AndThenTo(TimeSpan.FromMilliseconds(1000), value => (value * 2).ToString());

            completes.With(5);
            var result = completes.Await<string>(TimeSpan.FromMilliseconds(10));

            Assert.Equal("10", result);
        }
        
        [Fact]
        public void TestOtherwiseConsume()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());
            var failedResult = -1;
            
            completes
                .AndThenTo(5, v => Completes2.WithSuccess(v * 2))
                .OtherwiseConsume(failedValue => failedResult = failedValue);
            
            completes.With(5);

            var completed = completes.Await();
            
            Assert.Equal(5, failedResult);
            Assert.Equal(5, completed);
        }
        
        [Fact]
        public void TestAndThenToWithComplexType()
        {
            var completes = new BasicCompletes2<IUser>(new Scheduler());

            completes.AndThenTo(user => user.WithName("Tomasz"));

            completes.With(new User());

            var completed = completes.Await<UserState>();
            
            Assert.Equal("Tomasz", completed.Name);
        }

        [Fact(Skip = "Not yet implemented")]
        public void TestAndThenToWithComplexTypes()
        {
            var scheduler = new Scheduler();
            var completes = new BasicCompletes2<IUser>(scheduler);
            var nestedCompletes = new BasicCompletes2<UserState>(scheduler);

            completes
                .AndThenTo(user => user.WithName("Tomasz"))
                .OtherwiseConsume(noUser => nestedCompletes.With(new UserState(string.Empty, string.Empty, string.Empty)))
                .AndThenConsume(userState => {
                    nestedCompletes.With(userState);
                });

            completes.With(new User());

            var completed = completes.Await();
            
            Assert.Equal("1", ((User)completed).Name);
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
    }
    
    public interface IUser
    {
        ICompletes2<UserState> WithContact(string contact);
        ICompletes2<UserState> WithName(string name);
    }
    
    public class User : IUser
    {
        private UserState _userState;

        public string Name => _userState.Name;

        public User()
        {
            _userState = new UserState("1", "1", "1");
        }
        
        public ICompletes2<UserState> WithContact(string contact)
        {
            return Completes2.WithSuccess(_userState.WithContact(contact));
        }

        public ICompletes2<UserState> WithName(string name)
        {
            return Completes2.WithSuccess(_userState.WithName(name));
        }
    }

    public class UserState
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
}
