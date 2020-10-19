// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vlingo.Common.Serialization
{
    public static class JsonSerialization
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        
        public static T Deserialized<T>(string serialization)
            => JsonConvert.DeserializeObject<T>(serialization, Settings) ??
               throw new InvalidOperationException($"Cannot deserialize '{serialization}' to type {nameof(T)}");
        
        public static T Deserialized<T>(string serialization, JsonSerializerSettings settings)
            => JsonConvert.DeserializeObject<T>(serialization, settings) ??
               throw new InvalidOperationException($"Cannot deserialize '{serialization}' to type {nameof(T)}");

        public static object? Deserialized(string serialization, Type type)
            => JsonConvert.DeserializeObject(serialization, type, Settings);
        
        public static object? Deserialized(string serialization, Type type, JsonSerializerSettings settings)
            => JsonConvert.DeserializeObject(serialization, type, settings);

        public static List<T>? DeserializedList<T>(string serialization)
            => JsonConvert.DeserializeObject<List<T>>(serialization, Settings);
        
        public static List<T>? DeserializedList<T>(string serialization, JsonSerializerSettings settings)
            => JsonConvert.DeserializeObject<List<T>>(serialization, settings);

        public static string Serialized<T>(T instance)
            => JsonConvert.SerializeObject(instance, Settings);
        
        public static string Serialized<T>(T instance, JsonSerializerSettings settings)
            => JsonConvert.SerializeObject(instance, settings);
    }
}
