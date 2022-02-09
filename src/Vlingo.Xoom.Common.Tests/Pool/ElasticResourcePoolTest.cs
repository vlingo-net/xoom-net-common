// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using Vlingo.Xoom.Common.Pool;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Pool
{
    public class ElasticResourcePoolTest : ResourcePoolTest
    {
        private readonly ElasticResourcePool<int, Nothing> _pool;

        [Fact]
        public void TestInitialState()
        {
            Assert.Equal(10, _pool.Size);
            var stats = _pool.Stats();
            Assert.Equal(_pool.Size, stats.Idle);
            Assert.Equal(10, stats.Allocations);
            Assert.Equal(0, stats.Evictions);
            Assert.Equal(0, stats.InUse);
        }
        
        [Theory]
        [InlineData(100)]
        [InlineData(200)]
        [InlineData(1000)]
        public async Task TestConcurrentClients(int clients)
        {
            await TestConcurrent(_pool, clients);
            var stats = _pool.Stats();
            Assert.Equal(stats.Allocations - stats.Idle, stats.Evictions);
            Assert.Equal(0, stats.InUse);
        }

        public ElasticResourcePoolTest() => _pool = new ElasticResourcePool<int, Nothing>(ElasticResourcePool<int, Nothing>.Config.Of(10), new TestResourceFactory());
    }
}