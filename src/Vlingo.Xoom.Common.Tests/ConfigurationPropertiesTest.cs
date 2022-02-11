// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using System.IO;
using Xunit;

namespace Vlingo.Xoom.Common.Tests;

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