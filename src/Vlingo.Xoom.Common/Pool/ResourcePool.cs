// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Common.Pool;

/// <summary>
/// An abstract <see cref="IResourcePool{TResource,TArguments}"/> that implements <code>TResource Acquire(TArguments arguments)</code>
/// using the default arguments from <see cref="IResourceFactory{TResource,TArguments}"/> <code>DefaultArguments</code>.
/// </summary>
/// <typeparam name="TResource">The type of the pooled resource</typeparam>
/// <typeparam name="TArguments">The type of the arguments to the acquire method</typeparam>
public abstract class ResourcePool<TResource, TArguments> : IResourcePool<TResource, TArguments>
{
    protected IResourceFactory<TResource, TArguments> Factory;

    public ResourcePool(IResourceFactory<TResource, TArguments> factory) => Factory = factory;

    /// <summary>
    /// Uses default arguments to acquire resource object.
    /// </summary>
    /// <returns>A resource object with default arguments</returns>
    public virtual TResource Acquire() => Acquire(Factory.DefaultArguments);

    public abstract TResource Acquire(TArguments arguments);

    public abstract void Release(TResource resource);

    public abstract int Size { get; }

    public abstract ResourcePoolStats Stats();
}