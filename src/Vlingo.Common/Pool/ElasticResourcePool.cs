// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;

namespace Vlingo.Common.Pool
{
    /// <summary>
    /// An elastic <see cref="IResourcePool{TResource,TArguments}"/> implementation backed by a <see cref="ConcurrentQueue{T}"/>.
    /// <para>
    ///  This implementation will allocate new resource objects as needed in case the pool is exhausted.
    /// </para>
    /// <para>
    /// Resource objects will return to the pool only when the idle to inUse ratio is less than the desired minimum idle resources.
    /// When the idle to inUse ratio is higher than the minimum idle resources, the returning resources are evicted. Compaction
    /// of the resource cache is automatically triggered when the idle count is greater than the number of desired resources.
    /// </para>
    /// <para>
    /// Compaction attempts to half the size of the idle cache, reaching the desired minimum resource count as the cache drains.
    /// </para>
    /// <para>
    /// See <see cref="Config"/> for configuration details.
    /// </para>
    /// <para>
    ///  Resource object allocation, reset and destruction is managed by <see cref="IResourceFactory{TResource,TArguments}"/> implementation for the same type of Resource and Arguments.
    /// </para>
    /// </summary>
    /// <typeparam name="TResource">The type of resource</typeparam>
    /// <typeparam name="TArguments">The type of arguments for the <see cref="IResourceFactory{TResource,TArguments}"/></typeparam>
    public class ElasticResourcePool<TResource, TArguments> : ResourcePool<TResource, TArguments>
    {
        private readonly AtomicInteger idle = new AtomicInteger(0);
        private readonly AtomicInteger allocations = new AtomicInteger(0);
        private readonly AtomicInteger evictions = new AtomicInteger(0);

        private ConcurrentQueue<TResource> cache = new ConcurrentQueue<TResource>();

        private int minIdle;
        
        /// <summary>
        /// Creates an <see cref="ElasticResourcePool{TResource,TArguments}"/> instance initialized to pool from <see cref="Config"/> resource objects.
        /// <para>
        /// Resource object instances will be created using <see cref="IResourceFactory{TResource,TArguments}"/> <code>Create(object)</code>
        /// with the default arguments specified in <see cref="IResourceFactory{TResource,TArguments}"/> default arguments.
        /// </para>
        /// </summary>
        /// <param name="config">The <see cref="Config"/></param>
        /// <param name="factory">The resource object factory</param>
        public ElasticResourcePool(Config config, IResourceFactory<TResource, TArguments> factory) : this(config.MinIdle, factory)
        {
        }

        private ElasticResourcePool(int minIdle, IResourceFactory<TResource, TArguments> factory) : base(factory)
        {
            this.minIdle = minIdle;

            Initialize();
        }

        /// <summary>
        /// Gets a resource object from the pool and resets it,
        /// or creates a new one if the pool is exhausted.
        /// </summary>
        /// <param name="arguments">The arguments</param>
        /// <returns>A resource object</returns>
        public override TResource Acquire(TArguments arguments)
        {
            if (!cache.TryDequeue(out var resource))
            {
                allocations.IncrementAndGet();
                resource = Factory.Create(arguments);
            }
            else
            {
                idle.DecrementAndGet();
                resource = Factory.Reset(resource, arguments);
            }
            return resource;
        }

        /// <summary>
        /// Releases the object back into the pool, or evicts it when the idle to inUse ratio
        /// is higher than the desired minimum number of resources.
        /// </summary>
        /// <param name="resource">The resource object</param>
        public override void Release(TResource resource)
        {
            var stats = Stats();
            if (stats.IdleToInUse < minIdle)
            {
                idle.IncrementAndGet();
                cache.Enqueue(resource);
            }
            else if (idle.Get() > minIdle)
            {
                Evict(resource);
                Compact();
            }
            else
            {
                Evict(resource);
            }
        }
        
        public override ResourcePoolStats Stats() => new ResourcePoolStats(allocations.Get(), evictions.Get(), idle.Get());

        public override int Size => cache.Count;

        private void Initialize()
        {
            for (var i = 0; i < minIdle; i++)
            {
                allocations.IncrementAndGet();
                Cache(Factory.Create(Factory.DefaultArguments));
            }
        }

        private void Cache(TResource resource)
        {
            idle.IncrementAndGet();
            cache.Enqueue(resource);
        }
        
        private void Evict(TResource resource)
        {
            evictions.IncrementAndGet();
            Factory.Destroy(resource);
        }

        private void Compact()
        {
            while (idle.Get() > Target())
            {
                if (!cache.TryDequeue(out var resource))
                {
                    return;
                }
                
                if (idle.GetAndDecrement() > Target())
                {
                    Evict(resource);
                }
                else
                {
                    idle.IncrementAndGet();
                    cache.Enqueue(resource);
                }
            }
        }

        private int Target() 
        {
            return Math.Max(minIdle, (int) (idle.Get() * 0.5));
        }
        
        /// <summary>
        /// The <see cref="ElasticResourcePool{TResource,TArguments}"/> configuration parameters
        /// </summary>
        public class Config
        {
            public int MinIdle { get; }

            public static Config Of(int minIdle) => new Config(minIdle);
            
            /// <summary>
            /// Constructs the configuration with the minimum
            /// </summary>
            /// <param name="minIdle">The minimum number of resource objects to retain in the idle cache</param>
            private Config(int minIdle)
            {
                MinIdle = minIdle;
            }
        }
    }
}