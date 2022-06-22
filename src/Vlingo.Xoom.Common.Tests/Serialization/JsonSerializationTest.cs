// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vlingo.Xoom.Common.Serialization;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Serialization;

public class JsonSerializationTest
{
    private readonly JsonSerializerSettings _settings;
    
    [Fact]
    public void TestItSerializesPlainText() => 
        AssertJson("\"plain text\"", JsonSerialization.Serialized("plain text"));

    [Fact]
    public void TestItDeserializesPlainTextByClass() => 
        Assert.Equal("plain text (class)", JsonSerialization.Deserialized("\"plain text (class)\"", typeof(string)));

    [Fact]
    public void TestItDeserializesPlainTextByType() => 
        Assert.Equal("plain text (type)", JsonSerialization.Deserialized<string>("\"plain text (type)\""));

    [Fact]
    public void TestItSerializesNumbers() => AssertJson("42", JsonSerialization.Serialized(42));

    [Fact]
    public void TestItDeserializesNumbersByClass() => 
        Assert.Equal(13, (int)JsonSerialization.Deserialized("13", typeof(int)));

    [Fact]
    public void TestItDeserializesNumbersByType() => 
        Assert.Equal(13, JsonSerialization.Deserialized<int>("13"));

    [Fact]
    public void TestItDeserializesPreviouslySerializedObject()
    {
        var expected = new TestSerializationSubject(new TestSerializationChild("Alice"), "lorem ipsum", 13);
        var deserialized = JsonSerialization.Deserialized(JsonSerialization.Serialized(expected, _settings),
            typeof(TestSerializationSubject), _settings);
        Assert.Equal(expected, deserialized);
    }
    
    [Fact]
    public void TestItSerializesObjects()
    {
        var serialized = JsonSerialization.Serialized(new TestSerializationSubject(new TestSerializationChild("Alice"),
                "lorem ipsum", 13), _settings);
        var expected = "{\"_child\":{\"_name\":\"Alice\"},\"_text\":\"lorem ipsum\",\"_number\":13}";
        AssertJson(expected, serialized);
    }
    
    [Fact]
    public void TestItDeserializesObjectsByClass()
    {
        var deserialized = JsonSerialization.Deserialized(
            "{\"child\":{\"name\":\"Bob\"},\"text\":\"lorem ipsum\",\"number\":42}", typeof(TestSerializationSubject), _settings);
        var expected = new TestSerializationSubject(new TestSerializationChild("Bob"), "lorem ipsum", 42);
        Assert.Equal(expected, deserialized);
    }
    
    [Fact]
    public void TestItDeserializesObjectsByType()
    {
        var expected =
            new TestSerializationSubject(new TestSerializationChild("Bob"), "lorem ipsum", 42);
        var deserialized = JsonSerialization.Deserialized<TestSerializationSubject>(
            "{\"child\":{\"name\":\"Bob\"},\"text\":\"lorem ipsum\",\"number\":42}", _settings);
        Assert.Equal(expected, deserialized);
    }

    [Fact]
    public void TestItSerializesLists()
    {
        var list =
            new List<TestSerializationSubject> { new TestSerializationSubject(new TestSerializationChild("Bob"), 
                "lorem ipsum", 42) };
        var serialized = JsonSerialization.Serialized(list, _settings);
        AssertJson("[{\"_child\":{\"_name\":\"Bob\"},\"_text\":\"lorem ipsum\",\"_number\":42}]", serialized);
    }
    
    [Fact]
    public void TestItDeserializesLists()
    {
        var expected = new List<TestSerializationSubject> { new TestSerializationSubject(new TestSerializationChild("Bob"), 
                        "lorem ipsum", 42) };
        var deserialized = JsonSerialization.DeserializedList<TestSerializationSubject>(
            "[{\"child\":{\"name\":\"Bob\"},\"text\":\"lorem ipsum\",\"number\":42}]", _settings);
        Assert.Equal(expected, deserialized);
    }
    
    [Fact]
    public void TestItSerializesClass()
    {
        var type = typeof(TestSerializationSubject);
        var serialized = JsonSerialization.Serialized(type);
        var expected =
            "\"Vlingo.Xoom.Common.Tests.Serialization.JsonSerializationTest+TestSerializationSubject, Vlingo.Xoom.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"";
        AssertJson(expected, serialized);
    }
    
    [Fact]
    public void TestItDeserializesClass()
    {
        var serialized =
            "\"Vlingo.Xoom.Common.Tests.Serialization.JsonSerializationTest+TestSerializationSubject, Vlingo.Xoom.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"";
        var deserialized = JsonSerialization.Deserialized<Type>(serialized, _settings);
        Assert.Equal("TestSerializationSubject", deserialized.Name);
    }
    
    [Fact]
    public void TestItThrowsIfClassIsNotFoundWhileDeserializing()
    {
        Assert.Throws<JsonSerializationException>(() => {
            var serialized = "\"Vlingo.Xoom.Common.Tests.Serialization.JsonSerializationTest+MissingClass, Vlingo.Xoom.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullVlingo.Xoom.Common.Tests.Serialization.JsonSerializationTest+MissingClass, Vlingo.Xoom.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"";
            JsonSerialization.Deserialized<Type>(serialized, _settings);
        });
    }
    
    [Fact]
    public void TestItSerializesMicrosoftDate()
    {
        var expected = "\"\\/Date(-59591721600000)\\/\"";
        _settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
        var serialized = JsonSerialization.Serialized(new DateTime(81, 8, 12, 0, 0, 0, 0, DateTimeKind.Utc), _settings);
        _settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        AssertJson(expected, serialized);
    }
    
    [Fact]
    public void TestItSerializesIsoDate()
    {
        var expected = "\"0081-08-12T00:00:00\"";
        var serialized = JsonSerialization.Serialized(new DateTime(81, 8, 12, 0, 0, 0, 0), _settings);
        AssertJson(expected, serialized);
    }
    
    [Fact]
    public void TestItDeserializesMicrosoftDate()
    {
        var expected = new DateTime(81, 8, 12);
        _settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
        var deserialized = JsonSerialization.Deserialized<DateTime>("\"\\/Date(-59591721600000)\\/\"", _settings);
        _settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        Assert.Equal(expected, deserialized.ToUniversalTime());
    }
    
    [Fact]
    public void TestItDeserializesIsoDate()
    {
        var expected = new DateTime(81, 8, 12);
        _settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
        var deserialized = JsonSerialization.Deserialized<DateTime>("\"0081-08-12T00:00:00\"", _settings);
        _settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        Assert.Equal(expected, deserialized);
    }

    public JsonSerializationTest()
    {
        _settings = new JsonSerializerSettings();
        _settings.ContractResolver = new PrivateFieldResolver();
    }

    private void AssertJson(string expected, string actual) =>
        Assert.True(JToken.DeepEquals(JToken.Parse(expected), JToken.Parse(actual)));

    class TestSerializationSubject
    {
        private readonly TestSerializationChild _child;
        private readonly string _text;
        private readonly int _number;

        public TestSerializationSubject(TestSerializationChild child, string text, int number)
        {
            _child = child;
            _text = text;
            _number = number;
        }

        private TestSerializationSubject()
        {
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            var that = (TestSerializationSubject)o;
            return _number == that._number && _child.Equals(that._child) && _text.Equals(that._text);
        }
        
        public override int GetHashCode() => HashCode.Combine(_child, _text, _number);
    }

    class TestSerializationChild
    {
        private readonly string _name;

        public TestSerializationChild(string name) => _name = name;

        private TestSerializationChild()
        {
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            var that = (TestSerializationChild)o;
            return _name.Equals(that._name);
        }

        public override int GetHashCode() => _name.GetHashCode();
    }
}