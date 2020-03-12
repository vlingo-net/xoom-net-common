// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Version;

namespace Vlingo.Common.Message
{
    public interface IMessage
    {
        string Id { get; }
        DateTimeOffset OccurredOn { get; }
        T Payload<T>();
        string Type { get; }
        string Version { get; }
        SemanticVersion SemanticVersion { get; }
    }

    public static class MessageExtensions
    {
        public static SemanticVersion From(this IMessage message) => SemanticVersion.From(message.Version);
    }
}
