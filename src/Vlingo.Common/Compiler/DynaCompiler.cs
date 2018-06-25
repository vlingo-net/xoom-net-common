// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using static Vlingo.Common.Compiler.DynaFile;

namespace Vlingo.Common.Compiler
{
    public class DynaCompiler
    {
        public DynaCompiler()
        {
            // TODO: set and initialize Roslyn compiler
        }

        public Type Compile(Input input)
        {
            try
            {
                string sourceCode;
                using (var stream = input.SourceFile.OpenText())
                {
                    sourceCode = stream.ReadToEnd();
                }

                var tree = SyntaxFactory.ParseSyntaxTree(sourceCode);

                var assembliesToLoad = new HashSet<Assembly>
                {
                    typeof(object).Assembly,
                    input.Protocol.Assembly,
                    Assembly.Load("mscorlib"),
                };

                input.Protocol.Assembly
                    .GetReferencedAssemblies()
                    .Select(x => Assembly.Load(x))
                    .ToList()
                    .ForEach(x => assembliesToLoad.Add(x));

                var metadataRefs = assembliesToLoad.Select(x => MetadataReference.CreateFromFile(x.Location));

                var compilation = CSharpCompilation
                    .Create(input.FullyQualifiedClassName)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(metadataRefs)
                    .AddSyntaxTrees(tree);

                byte[] byteCode = null;

                using (var ilStream = new MemoryStream())
                {
                    var compilationResult = compilation.Emit(ilStream);
                    ilStream.Seek(0, SeekOrigin.Begin);
                    byteCode = ilStream.ToArray();
                }

                Persist(input, byteCode);

                return input.ClassLoader.AddDynaClass(input.FullyQualifiedClassName, byteCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dynamically generated class source for {input.FullyQualifiedClassName} did not compile because: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            throw new ArgumentException($"Dynamically generated class source did not compile: {input.FullyQualifiedClassName}");
        }

        private void Persist(Input input, byte[] byteCode)
        {
            if (!input.Persist)
            {
                return;
            }

            string relativePathToClass = ToFullPath(input.FullyQualifiedClassName);
            string pathToCompiledClass = ToNamespacePath(input.FullyQualifiedClassName);
            string rootOfGenerated = input.Type == DynaType.Main ? RootOfMainClasses : RootOfTestClasses;
            var directory = new DirectoryInfo(rootOfGenerated + pathToCompiledClass);
            if (!directory.Exists)
            {
                directory.Create();
            }
            string pathToClass = rootOfGenerated + relativePathToClass + ".dll";

            PersistDynaClass(pathToClass, byteCode);
        }
    }
}
