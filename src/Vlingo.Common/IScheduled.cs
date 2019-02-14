// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

namespace Vlingo.Common
{
    public interface IScheduled
    {
        void IntervalSignal(IScheduled scheduled, object data);
    }

    public interface IScheduledAsync : IScheduled
    {
        Task IntervalSignalAsync(IScheduled scheduled, object data);
    }
}
