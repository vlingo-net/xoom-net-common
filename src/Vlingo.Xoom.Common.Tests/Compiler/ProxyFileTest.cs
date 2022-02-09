// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using Vlingo.Xoom.Common.Compiler;
using Xunit;

namespace Vlingo.Xoom.Common.Tests.Compiler
{
    public class ProxyFileTest : DynaTest, IDisposable
    {
        private readonly DirectoryInfo _parentPathFile;
        private readonly string _pathToSource;
        private readonly FileInfo _pathToSourceFile;

        // SetUp
        public ProxyFileTest()
        {
            var parentPath = Path.Combine(Path.GetTempPath(), DynaFile.ToNamespacePath(ClassName));
            _parentPathFile = new DirectoryInfo(parentPath);
            _pathToSource = Path.Combine(Path.GetTempPath(), $"{DynaFile.ToFullPath(ClassName)}.cs");
            _pathToSourceFile = new FileInfo(_pathToSource);
        }        

        [Fact]
        public void TestPersistProxyClassSource()
        {
            _parentPathFile.Create();
            DynaFile.PersistDynaClassSource(_pathToSource, Source);
            using (var input = _pathToSourceFile.OpenText())
            {
                Assert.Equal(Source, input.ReadToEnd());
            }
        }

        [Fact]
        public void TestToFullPath()
        {
            var path = DynaFile.ToFullPath("Vlingo.Xoom.Actors.Startable");
            var expected = string.Format("Vlingo{0}Xoom{0}Actors{0}Startable", Path.DirectorySeparatorChar);
            Assert.Equal(expected, path);
        }

        [Fact]
        public void TestToPackagePath()
        {
            var path = DynaFile.ToNamespacePath("Vlingo.Xoom.Actors.Startable");
            Assert.Equal($"Vlingo{Path.DirectorySeparatorChar}Xoom{Path.DirectorySeparatorChar}Actors", path);
        }

        // TearDown
        public void Dispose()
        {
            if (_pathToSourceFile.Exists)
            {
                _pathToSourceFile.Delete();
            }

            if (_parentPathFile.Exists)
            {
                _parentPathFile.Delete(); 
            }
        }
    }
}
