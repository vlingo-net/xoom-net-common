using System;
using System.IO;
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
            // generate and compile the source
            byte[] classData = null;
            // or persisted path
            return input.ClassLoader.AddDynaClass(input.FullyQualifiedClassName, classData);
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
