using System;
using System.IO;

namespace Vlingo.Common.Compiler
{
    public class Input
    {
        public Input(
            Type protocol,
            string fullyQualifiedClassName,
            string source,
            FileInfo sourceFile,
            DynaClassLoader classLoader,
            DynaType type,
            bool persist)
        {
            Protocol = protocol;
            FullyQualifiedClassName = fullyQualifiedClassName;
            Source = source;
            SourceFile = sourceFile;
            ClassLoader = classLoader;
            Type = type;
            Persist = persist;
        }

        public Type Protocol { get; }
        public string FullyQualifiedClassName { get; }
        public string Source { get; }
        public FileInfo SourceFile { get; }
        public DynaClassLoader ClassLoader { get; }
        public DynaType Type { get; }
        public bool Persist { get; }
    }
}
