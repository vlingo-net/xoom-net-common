using System;
using System.IO;
using Vlingo.Common.Compiler;
using Xunit;

namespace Vlingo.Common.Tests.Compiler
{
    public class ProxyFileTest : DynaTest, IDisposable
    {
        private readonly string parentPath;
        private readonly DirectoryInfo parentPathFile;
        private readonly string pathToSource;
        private readonly FileInfo pathToSourceFile;

        // SetUp
        public ProxyFileTest()
        {
            parentPath = Path.Combine(Path.GetTempPath(), DynaFile.ToNamespacePath(ClassName));
            parentPathFile = new DirectoryInfo(parentPath);
            pathToSource = Path.Combine(Path.GetTempPath(), $"{DynaFile.ToFullPath(ClassName)}.cs");
            pathToSourceFile = new FileInfo(pathToSource);
        }        

        [Fact]
        public void TestPersistProxyClassSource()
        {
            parentPathFile.Create();
            DynaFile.PersistDynaClassSource(pathToSource, Source);
            using (var input = pathToSourceFile.OpenText())
            {
                Assert.Equal(Source, input.ReadToEnd());
            }
        }

        [Fact]
        public void TestToFullPath()
        {
            var path = DynaFile.ToFullPath("Vlingo.Actors.Startable");
            var expected = string.Format("Vlingo{0}Actors{0}Startable", Path.DirectorySeparatorChar);
            Assert.Equal(expected, path);
        }

        [Fact]
        public void TestToPackagePath()
        {
            var path = DynaFile.ToNamespacePath("Vlingo.Actors.Startable");
            Assert.Equal($"Vlingo{Path.DirectorySeparatorChar}Actors", path);
        }

        // TearDown
        public void Dispose()
        {
            if (pathToSourceFile.Exists)
            {
                pathToSourceFile.Delete();
            }

            if (parentPathFile.Exists)
            {
                parentPathFile.Delete(); 
            }
        }
    }
}
