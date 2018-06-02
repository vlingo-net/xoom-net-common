using System;
using System.IO;
using System.Reflection;
using Vlingo.Common.Compiler;
using Xunit;

using static Vlingo.Common.Compiler.DynaFile;

namespace Vlingo.Common.Tests.Compiler
{
    public class DynaClassLoaderTest : DynaTest
    {
        public Assembly Assemby { get; private set; }

        [Fact]
        public void TestDynaClassLoader()
        {
            var classLoader = new DynaClassLoader(typeof(DynaClassLoader).GetAssemblyLoadContext());
            var dynaNamingClassType = classLoader.LoadClass("Vlingo.Common.Compiler.DynaNaming");
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
            var actorDynaClass = classLoader.LoadClass("Vlingo.Common.Compiler.DynaFile");
            Assert.NotNull(actorDynaClass);
        }
    }

    public interface ITestInterface
    {
        int Test();
    }
}
