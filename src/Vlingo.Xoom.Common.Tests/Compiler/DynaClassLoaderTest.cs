// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Reflection;
using Vlingo.Xoom.Common.Compiler;
using Xunit;
using static Vlingo.Xoom.Common.Compiler.DynaFile;

namespace Vlingo.Xoom.Common.Tests.Compiler;

public class DynaClassLoaderTest : DynaTest
{
    public Assembly Assemby { get; private set; }

    [Fact]
    public void TestDynaClassLoader()
    {
        var classLoader = new DynaClassLoader();
        var dynaNamingClassType = classLoader.LoadClass("Vlingo.Xoom.Common.Compiler.DynaNaming");
        Assert.NotNull(dynaNamingClassType);

        var relativeTargetFile = ToFullPath(ClassName);
        var pathToGeneratedSource = ToNamespacePath(ClassName);
        var directory = GeneratedTestSources + pathToGeneratedSource;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var pathToSource = $"{GeneratedTestSources}{relativeTargetFile}.cs";

        var input = new Input(
            typeof(ITestInterface),
            ClassName,
            ClassName,
            Source,
            PersistDynaClassSource(pathToSource, Source),
            classLoader,
            DynaType.Test,
            false);

        new DynaCompiler().Compile(input);

        // load a brand new class just added to the DynaClassLoader
        var testClass = classLoader.LoadClass(ClassName);
        Assert.NotNull(testClass);
        var instance = (ITestInterface)Activator.CreateInstance(testClass);
        Assert.NotNull(instance);
        Assert.Equal(1, instance.Test());

        // load another class from the default/parent ClassLoader
        var actorDynaClass = classLoader.LoadClass("Vlingo.Xoom.Common.Compiler.DynaFile");
        Assert.NotNull(actorDynaClass);
    }
        
    [Fact]
    public void TestLoadImplementationDifferentNamespace()
    {
        var protocolName = typeof(IRunnable);
        var classLoader = new DynaClassLoader();
            
        // Vlingo.Xoom.Common.Runnable__Proxy is what is being called if the interface is implemented in
        // different namespace than the implementation.
        var dynaNamingClassType = classLoader.LoadClass("Vlingo.Xoom.Common.Runnable__Proxy", protocolName);
        Assert.NotNull(dynaNamingClassType);
    }
}

public interface ITestInterface
{
    int Test();
}
    
public class Runnable__Proxy : IRunnable
{
    public void Run()
    {
    }
}