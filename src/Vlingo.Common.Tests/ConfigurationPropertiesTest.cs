using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

namespace Vlingo.Common.Tests
{
    public class TestConsumer : ConfigurationProperties
    {}

    public class ConfigurationPropertiesTest
    {
        [Fact]
        public void TestThatConfigurationFileCanBeReadSuccessfully()
        {
            var testConsumer = new TestConsumer();
            testConsumer.Load(new FileInfo("vlingo-test.json"));

            Assert.Equal(13, testConsumer.Keys.Count);

            Assert.Equal("vlingo-net/actors",
                testConsumer.GetProperty("plugin.consoleLogger.name"));

            Assert.True(Convert.ToBoolean(
                testConsumer.GetProperty("plugin.name.consoleLogger")));

            Assert.Equal(1.5m, Convert.ToDecimal(
                testConsumer.GetProperty("plugin.queueMailbox.numberOfDispatchersFactor"),
                CultureInfo.InvariantCulture));

            Assert.Equal(1, Convert.ToInt32(
                testConsumer.GetProperty("plugin.queueMailbox.dispatcherThrottlingCount")));

        }

    }
}
