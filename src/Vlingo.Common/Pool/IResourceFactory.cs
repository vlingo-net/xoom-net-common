// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Common.Pool
{
    /// <summary>
    /// Manages creation, reset and destruction of resource objects created
    /// in combination with a <see cref="IResourcePool{TResource,TArguments}"/> implementation.
    /// </summary>
    /// <typeparam name="TResource">The type of resource</typeparam>
    /// <typeparam name="TArguments">The type fo arguments to create and reset methods</typeparam>
    public interface IResourceFactory<TResource, TArguments>
    {
        /// <summary>
        /// Creates a resource object.
        /// </summary>
        /// <param name="arguments">The arguments</param>
        /// <returns>A new resource object</returns>
        TResource Create(TArguments arguments);
        
        /// <summary>
        /// Gets the default arguments to use for initial resource creation.
        /// </summary>
        TArguments DefaultArguments { get; }
        
        /// <summary>
        /// Resets a resource object for others to use.
        /// </summary>
        /// <param name="resource">The resource object</param>
        /// <param name="arguments">The arguments</param>
        /// <returns>The reset resource object</returns>
        TResource Reset(TResource resource, TArguments arguments);
        
        /// <summary>
        /// Destroys a resource object.
        /// </summary>
        /// <param name="resource">The resource object</param>
        void Destroy(TResource resource);
    }
}