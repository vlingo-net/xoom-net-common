// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.Common.Tests
{
    public class OutcomeTest
    {
        [Fact]
        public void TestThatASuccessfulOutcomeCanBeResolved()
        {
            var initialValue = RandomInt();
            var failedValue = RandomInt();
            var successInt = RandomInt();
            var expected = successInt * initialValue;

            var outcome = Success.Of<Exception, int>(initialValue).Resolve(ex => failedValue, val => val * successInt);

            Assert.Equal(expected, outcome);
        }

        [Fact]
        public void TestThatASuccessfulOutcomeCanBeComposed()
        {
            var initialValue = RandomInt();
            var failedValue = RandomInt();
            var successInt = RandomInt();
            var expected = successInt * initialValue;

            var outcome = Success.Of<Exception, int>(initialValue).AndThen(val => val * successInt).Get();

            Assert.Equal(expected, outcome);
        }

        [Fact]
        public void TestThatASuccessfulOutcomeCanBeComposedWithANewOutcome()
        {
            var initialValue = RandomInt();
            var failedValue = RandomInt();
            var successInt = RandomInt();
            var expected = successInt * initialValue;

            var outcome = Success.Of<Exception, int>(initialValue).AndThenTo(val => Success.Of<Exception, int>(val * successInt)).Get();

            Assert.Equal(expected, outcome);
        }

        [Fact]
        public void TestThatAtLeastConsumedIsCalledWhenASuccess()
        {
            var currentValue = new AtomicInteger(0);
            var initialValue = RandomInt();

            Success.Of<Exception, int>(initialValue).AtLeastConsume(currentValue.Set);

            Assert.Equal(initialValue, currentValue.Get());
        }

        [Fact]
        public void TestThatOtherwiseIsNotCalledWhenSuccess()
        {
            var currentValue = new AtomicInteger(0);
            var initialValue = RandomInt();

            var success = Success.Of<ApplicationException, int>(initialValue);
            var otherwise = success.Otherwise(ex =>
            {
                currentValue.Set(initialValue);
                return initialValue;
            });

            Assert.Equal(0, currentValue.Get());
            Assert.Equal(success, otherwise);
        }

        [Fact]
        public void TestThatOtherwiseIntoIsNotCalledWhenSuccess()
        {
            var currentValue = new AtomicInteger(0);
            var initialValue = RandomInt();

            var success = Success.Of<ApplicationException, int>(initialValue);
            var otherwise = success.OtherwiseTo(ex =>
            {
                currentValue.Set(initialValue);
                return Success.Of<ApplicationException, int>(initialValue);
            });

            Assert.Equal(0, currentValue.Get());
            Assert.Equal(success, otherwise);
        }

        [Fact]
        public void TestThatGetOrNullReturnsTheValueWhenSuccess()
        {
            var initialValue = RandomInt();
            var outcome = Success.Of<Exception, int>(initialValue).GetOrNull();

            Assert.Equal(initialValue, outcome);
        }

        [Fact]
        public void TestThatAFailureIsNotComposedWithAndThen()
        {
            var currentValue = new AtomicInteger(0);
            Failure.Of<Exception, int>(RandomException()).AndThen(currentValue.GetAndSet);

            Assert.Equal(0, currentValue.Get());
        }

        [Fact]
        public void TestThatAFailureIsNotComposedWithAndThenTo()
        {
            var currentValue = new AtomicInteger(0);
            Failure.Of<Exception, int>(RandomException())
                .AndThenTo(val => Success.Of<Exception, int>(currentValue.GetAndSet(val)));

            Assert.Equal(0, currentValue.Get());
        }

        [Fact]
        public void TestThatAFailureIsNotComposedWithAtLeastConsume()
        {
            var currentValue = new AtomicInteger(42);
            Failure.Of<Exception, int>(RandomException()).AtLeastConsume(currentValue.Set);

            Assert.Equal(42, currentValue.Get());
        }

        [Fact]
        public void TestThatAFailureIsRecoveredWithOtherwise()
        {
            var recordedValue = RandomInt();
            var outcome = Failure.Of<Exception, int>(RandomException())
                .Otherwise(ex => recordedValue)
                .Get();

            Assert.Equal(recordedValue, outcome);
        }

        [Fact]
        public void TestThatAFailureIsRecoveredWithOtherwiseInto()
        {
            var recordedValue = RandomInt();
            var outcome = Failure.Of<Exception, int>(RandomException())
                .OtherwiseTo(ex => Success.Of<ApplicationException, int>(recordedValue))
                .Get();

            Assert.Equal(recordedValue, outcome);
        }

        [Fact]
        public void TestThatGetOnAFailureThrowsTheCauseOfFailure()
        {
            var exception = RandomException();
            try
            {
                Failure.Of<Exception, int>(exception).Get();
                throw new InvalidOperationException("It should never reach here.");
            }
            catch (Exception ex)
            {
                Assert.Equal(exception, ex);
            }
        }

        [Fact]
        public void TestThatGetOrNullReturnsNullOnAFailure()
            => Assert.Null(Failure.Of<Exception, object>(RandomException()).GetOrNull());

        [Fact]
        public void TestThatResolveGoesThroughTheFailureBranchWhenFailedOutcome()
        {
            var currentValue = new AtomicInteger(0);
            var failedBranch = RandomInt();
            var successBranch = RandomInt();

            var outcome = Failure.Of<Exception, int>(RandomException())
                .Resolve(
                ex =>
                {
                    currentValue.Set(failedBranch);
                    return failedBranch;
                },
                currentValue.GetAndSet);

            Assert.Equal(outcome, currentValue.Get());
            Assert.Equal(failedBranch, currentValue.Get());
        }

        [Fact]
        public void TestThatASuccessOutcomeIsTransformedToAValidOptional()
        {
            var value = RandomInt();
            var outcome = Success.Of<Exception, int>(value).AsOptional();

            Assert.Equal(Optional.Of(value), outcome);
        }

        [Fact]
        public void TestThatAFailedOutcomeIsTransformedToAnEmptyOptional()
            => Assert.Equal(
                Optional.Empty<int>(),
                Failure.Of<Exception, int>(RandomException()).AsOptional());

        [Fact]
        public void TestThatASuccessOutcomeIsTransformedToASuccessCompletes()
        {
            var value = RandomInt();
            var completes = Success.Of<Exception, int>(value).AsCompletes();

            Assert.Equal(value, completes.Outcome);
        }

        [Fact]
        public void TestThatAFailedOutcomeIsTransformedToAFailedCompletes()
            => Assert.True(Failure.Of<Exception, int>(RandomException()).AsCompletes().HasFailed);

        [Fact]
        public void TestThatFilteringInASuccessOutcomeReturnsTheSameOutcome()
        {
            var outcome = Success.Of<Exception, int>(RandomInt());
            var filteredOutcome = outcome.Filter(_ => true);

            Assert.Equal(outcome.Get(), filteredOutcome.Get());
        }

        [Fact]
        public void TestThatFilteringOutASuccessOutcomeReturnsAFailedOutcome()
        {
            var outcome = Success.Of<Exception, int>(RandomInt());
            var filteredOutcome = outcome.Filter(_ => false);

            Assert.IsType<Failure<NoSuchElementException, int>>(filteredOutcome);
            Assert.Throws<NoSuchElementException>(() => filteredOutcome.Get());
        }

        [Fact]
        public void TestThatFilteringInAFailureOutcomeReturnsAFailedOutcome()
        {
            var outcome = Failure.Of<Exception, int>(RandomException());
            var filteredOutcome = outcome.Filter(_ => true);

            Assert.IsType<Failure<NoSuchElementException, int>>(filteredOutcome);
            Assert.Throws<NoSuchElementException>(() => filteredOutcome.Get());
        }

        [Fact]
        public void TestThatAlongWithReturnsBothSuccessesInATuple()
        {
            var first = Success.Of<Exception, int>(RandomInt());
            var second = Success.Of<Exception, int>(RandomInt());

            var wrapped = first.AlongWith(second);

            Assert.Equal(first.Get(), wrapped.Get().Item1);
            Assert.Equal(second.Get(), wrapped.Get().Item2);
        }

        [Fact]
        public void TestThatAlongWithReturnsFirstFailure()
        {
            var first = Failure.Of<InvalidOperationException, int>(new InvalidOperationException());
            var second = Success.Of<ApplicationException, int>(RandomInt());

            var wrapped = first.AlongWith(second);

            Assert.Throws<InvalidOperationException>(() => wrapped.Get());
        }

        [Fact]
        public void TestThatOtherwiseFailMapsTheException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Failure.Of<Exception, int>(RandomException())
                    .OtherwiseFail(f => new InvalidOperationException())
                    .Get();
            });
        }

        private static int RandomInt() => new Random().Next(1, 100);

        private static Exception RandomException() => new ApplicationException();
    }
}
