// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Xoom.Common.Pool;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Pool
{
    public abstract class ResourcePoolTest
    {
        private static readonly Random Random = new Random();

        public static async Task TestConcurrent(IResourcePool<int, Nothing> pool, int clients)
        {
            async Task Call()
            {
                int resource = -1;

                try
                {
                    do
                    {
                        resource = pool.Acquire();
                    } while (resource < 0);

                    await Task.Delay(Random.Next(10, 100));
                }
                finally
                {
                    pool.Release(resource);
                    var stats = pool.Stats();
                    Assert.Equal(stats.Allocations, stats.Evictions + stats.Idle + stats.InUse);
                }
            }

            var tasks = new List<Task>();
            for (var i = 0; i < clients; i++)
            {
                var task = Task.Run(Call);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            
            Assert.Equal(pool.Stats().Idle, pool.Size);
        }
    }
}