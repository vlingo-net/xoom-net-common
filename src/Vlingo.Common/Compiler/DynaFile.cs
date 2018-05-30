using System.IO;

namespace Vlingo.Common.Compiler
{
    public static class DynaFile
    {
        public const string GeneratedSources = "target/generated-sources/";
        public const string GeneratedTestSources = "target/generated-test-sources/";
        public const string RootOfMainClasses = "target/classes/";
        public const string RootOfTestClasses = "target/test-classes/";

        public static FileInfo PersistDynaClass(string pathToClass, byte[] dynaClass)
        {
            var file = new FileInfo(pathToClass);

            using (var stream = file.OpenWrite())
            {
                stream.Write(dynaClass, 0, dynaClass.Length);
            }

            return file;
        }

        public static FileInfo PersistDynaClassSource(string pathToSource, string dynaClassSource)
        {
            var file = new FileInfo(pathToSource);

            using (var stream = new StreamWriter(file.OpenWrite()))
            {
                stream.Write(dynaClassSource);
            }

            return file;
        }

        public static string ToFullPath(string fullyQualifiedClassName) => ToPath(fullyQualifiedClassName, true);

        public static string ToNamespacePath(string fullyQualifiedClassName) => ToPath(fullyQualifiedClassName, false);

        private static string ToPath(string fullyQualifiedClassName, bool includeClassName)
        {
            var lastDotIndex = fullyQualifiedClassName.LastIndexOf('.');
            var directoryPath = $"{fullyQualifiedClassName.Substring(0, lastDotIndex)}";
            if (includeClassName)
            {
                return $"{directoryPath}/{fullyQualifiedClassName.Substring(lastDotIndex + 1)}";
            }
            else
            {
                return directoryPath;
            }
        }
    }
}
