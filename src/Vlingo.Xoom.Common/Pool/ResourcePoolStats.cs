// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Common.Pool
{
    /// <summary>
    /// <see cref="IResourcePool{TResource,TArguments}"/> statistics
    /// </summary>
    public sealed class ResourcePoolStats
    {
        /// <summary>
        /// Constructs statistics state
        /// </summary>
        /// <param name="allocations">Number of resource allocations</param>
        /// <param name="evictions">Number of evicted resources</param>
        /// <param name="idle">Number of idle resources</param>
        public ResourcePoolStats(int allocations, int evictions, int idle)
        {
            Allocations = allocations;
            Evictions = evictions;
            Idle = idle;
            InUse = allocations - evictions - idle;
            IdleToInUse = (float) idle / Math.Max(1, InUse);
        }
        
        /// <summary>
        /// Gets number of resource allocations
        /// </summary>
        public int Allocations { get; }
        
        /// <summary>
        /// Gets number of evicted resources
        /// </summary>
        public int Evictions { get; }
        
        /// <summary>
        /// Gets number of idle resources
        /// </summary>
        public int Idle { get; }
        
        /// <summary>
        /// Gets number of resources assigned to consumers
        /// </summary>
        public int InUse { get; }
        
        /// <summary>
        /// Gets the idle to inUse ratio
        /// </summary>
        public float IdleToInUse { get; }

        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj != null && GetType() == obj.GetType())
            {
                var that = (ResourcePoolStats) obj;
                return Allocations == that.Allocations &&
                       Evictions == that.Evictions &&
                       Idle == that.Idle;
            }

            return false;
        }

        public override int GetHashCode() =>
            31 * Allocations.GetHashCode() + Evictions.GetHashCode() + Idle.GetHashCode();

        public override string ToString() =>
            $"ResourcePoolStats(allocations: {Allocations}, evictions: {Evictions}, idle: {Idle}, inUse: {InUse}, idleToInUse: {IdleToInUse:F})";
    }
}