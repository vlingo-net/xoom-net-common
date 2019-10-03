// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Common.Completion.Continuations
{
    internal class CompletesContinuation
    {
        internal readonly BasicCompletes Completes;

        public CompletesContinuation(BasicCompletes completes) => Completes = completes;

        internal void Run(BasicCompletes antecedentCompletes) => Completes.InnerInvoke(antecedentCompletes);
    }
}