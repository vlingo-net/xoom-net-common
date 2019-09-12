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

        [Fact(Skip = "Implementation in progress")]
        public void TestCompletesAfterFunction()
        {
            var completes = new BasicCompletes2<int>(0);
            completes.AndThen(value => value * 2);

            completes.With(5);

            Assert.Equal(10, completes.Outcome);
        }

        [Fact(Skip = "Implementation in progress")]
        public void TestCompletesAfterConsumer()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes2<int>(0);
            completes.AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(5, andThenValue);
        }

        [Fact(Skip = "Implementation in progress")]
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

        [Fact(Skip = "Implementation in progress")]
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

        [Fact(Skip = "Implementation in progress")]
        public void TestOutcomeBeforeTimeout()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(TimeSpan.FromMilliseconds(1000), value => value * 2)
                .AndThen(x => andThenValue = x);

            completes.With(5);
            completes.Await<int>(TimeSpan.FromMilliseconds(10));

            Assert.Equal(10, andThenValue);
        }

        [Fact(Skip = "Implementation in progress")]
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

            completes.Await<int>();

            Assert.True(completes.HasFailed);
            Assert.NotEqual(10, andThenValue);
            Assert.Equal(0, andThenValue);
        }

        [Fact(Skip = "Implementation in progress")]
        public void TestThatFailureOutcomeFails()
        {
            int andThenValue = -1, failureValue = 0;
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes
                .AndThen(-100, value => 2 * value)
                .AndThen(x => andThenValue = x)
                .Otherwise(x => failureValue = 1000);

            completes.With(-100);
            completes.Await<int>();

            Assert.True(completes.HasFailed);
            Assert.Equal(-1, andThenValue);
            Assert.Equal(1000, failureValue);
        }

        [Fact(Skip = "Implementation in progress")]
        public void TestThatExceptionOutcomeFails()
        {
            int failureValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());

            completes
                .AndThen(42, value => value * 2)
                .AndThen<int>(value => throw new ApplicationException((2 * value).ToString()))
                .RecoverFrom(e => failureValue = int.Parse(e.Message));

            completes.With(2);
            completes.Await<int>();

            Assert.True(completes.HasFailed);
            Assert.Equal(8, failureValue);
        }

        [Fact(Skip = "Implementation in progress")]
        public void TestThatExceptionHandlerDelayRecovers()
        {
            var failureValue = -1;
            var completes = new BasicCompletes2<int>(new Scheduler());
            completes
                .AndThen(0, value => value * 2)
                .AndThen<int>(value => throw new Exception($"{value * 2}"));

            completes.With(10);

            completes.RecoverFrom(e => failureValue = int.Parse(e.Message));

            completes.Await<int>();

            Assert.True(completes.HasFailed);
            Assert.Equal(40, failureValue);
        }

        [Fact(Skip = "Implementation in progress")]
        public void TestThatAwaitTimesOut()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());

            var completed = completes.Await<int>(TimeSpan.FromMilliseconds(10));

            completes.With(5);

            Assert.NotEqual(5, completed);
            Assert.Equal(default(int), completed);
        }

        [Fact(Skip = "Implementation in progress")]
        public void TestThatAwaitCompletes()
        {
            var completes = new BasicCompletes2<int>(new Scheduler());

            var thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(100);
                completes.With(5);
            }));
            thread.Start();

            var completed = completes.Await<int>();

            Assert.Equal(5, completed);
        }

        [Fact(Skip = "Implementation in progress")]
        public void TestAndThenToWithComplexTypes()
        {
            var scheduler = new Scheduler();
            var completes1 = new BasicCompletes2<int>(scheduler);
            completes1.AndThenTo(v => (v * 10).ToString());
            completes1.With(10);
            var result = completes1.Await<int>();

            Assert.Equal(10, result);

            var completes = new BasicCompletes2<IUser2>(scheduler);
            var nestedCompletes = new BasicCompletes2<UserState2>(scheduler);

            completes
                .AndThenTo(user => user.WithName("Tomasz"))
                .OtherwiseConsume(noUser => nestedCompletes.With(new UserState2(string.Empty, string.Empty, string.Empty)))
                .AndThenConsume(userState => {
                    nestedCompletes.With(userState);
                });

            completes.With<IUser>(new User());

            var completed = completes.Await<User>();
            
            Assert.Equal("1", completed.Name);
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
    
    public interface IUser2
    {
        ICompletes<UserState2> WithContact(string contact);
        ICompletes<UserState2> WithName(string name);
    }
    
    public class User2 : IUser2
    {
        private UserState2 _userState;

        public string Name => _userState.Name;

        public User2()
        {
            _userState = new UserState2("1", "1", "1");
        }
        
        public ICompletes<UserState2> WithContact(string contact)
        {
            return Completes2.WithSuccess(_userState.WithContact(contact));
        }

        public ICompletes<UserState2> WithName(string name)
        {
            return Completes2.WithSuccess(_userState.WithName(name));
        }
    }

    public class UserState2
    {
        public string Id { get; }
        public string Name { get; }
        public string Contact { get; }

        public UserState2 WithName(string name) => new UserState2(Id, name, Contact);
        
        public UserState2 WithContact(string contact) => new UserState2(Id, Name, contact);

        public UserState2(string id, string name, string contact)
        {
            Id = id;
            Name = name;
            Contact = contact;
        }
    }
}
