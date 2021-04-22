// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Common.Pool
{
    /// <summary>
    /// Resource pool
    /// </summary>
    /// <typeparam name="TResource">The type of the pooled resource</typeparam>
    /// <typeparam name="TArguments">The type of the arguments to the acquire method</typeparam>
    public interface IResourcePool<TResource, in TArguments>
    {
        /// <summary>
        /// Lends resource object from the pool.
        /// </summary>
        /// <returns>A resource</returns>
        TResource Acquire();
        
        /// <summary>
        /// Lends resource object from the pool.
        /// </summary>
        /// <param name="arguments">The arguments</param>
        /// <returns>A resource</returns>
        TResource Acquire(TArguments arguments);
        
        /// <summary>
        /// Returns the lease of a resource object to the pool.
        /// </summary>
        /// <param name="resource">The resource to return</param>
        void Release(TResource resource);
        
        /// <summary>
        /// Gets the number of available resource objects.
        /// </summary>
        int Size { get; }
        
        /// <summary>
        /// Statistics at the time of invocation.
        /// </summary>
        /// <returns><see cref="ResourcePoolStats"/></returns>
        ResourcePoolStats Stats();
    }
}