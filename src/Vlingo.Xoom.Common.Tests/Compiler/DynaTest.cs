// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Common.Tests.Compiler
{
    public abstract class DynaTest
    {
        protected const string ClassName = "Vlingo.Common.Tests.Compiler.TestProxy";
        protected const string Source = "namespace Vlingo.Common.Tests.Compiler { public class TestProxy : Vlingo.Common.Tests.Compiler.ITestInterface { public int Test() { return 1; } } }";
    }
}
