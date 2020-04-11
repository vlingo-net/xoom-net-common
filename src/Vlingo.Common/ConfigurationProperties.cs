// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vlingo.Common
{
    public abstract class ConfigurationProperties
    {
        private readonly IDictionary<string, string> _dictionary;
        
        public ConfigurationProperties() => _dictionary = new Dictionary<string, string>();

        public ConfigurationProperties(IDictionary<string, string> properties) => _dictionary = properties;

        public ICollection<string> Keys => _dictionary.Keys;

        public bool IsEmpty => _dictionary.Count == 0;

        public string? GetProperty(string key) => GetProperty(key, null);

        public string? GetProperty(string key, string? defaultValue)
        {
            if (_dictionary.TryGetValue(key.ToLowerInvariant(), out string value))
            {
                return value;
            }

            return defaultValue;
        }

        public void SetProperty(string key, string value) => _dictionary[key.ToLowerInvariant()] = value;

        public void Load(FileInfo configFile)
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(configFile.Name, false, true)
              .Build();

            var configurations = config.AsEnumerable().Where(c => c.Value != null);

            foreach (var configuration in configurations)
            {
                var k = configuration.Key.Replace(":", ".");
                var v = configuration.Value;
                SetProperty(k, v);
            }
        }
    }
}
