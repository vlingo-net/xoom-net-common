using System;
using System.IO;
using System.Runtime.Loader;

namespace Vlingo.Common.Compiler
{
    public class DynaClassLoader
    {
        private readonly AssemblyLoadContext context;

        public DynaClassLoader(AssemblyLoadContext context)
        {
            this.context = context;
        }

        internal Type AddDynaClass(string fullyQualifiedClassName, byte[] byteCode)
        {
            using(var stream = new MemoryStream(byteCode, false))
            {
                var assembly = context.LoadFromStream(stream);
                return assembly.GetType(fullyQualifiedClassName);
            }
        }

        internal Type AddDynaClass(string fullyQualifiedClassName, string dllPath)
        {
            var assembly = context.LoadFromAssemblyPath(dllPath);
            return assembly.GetType(fullyQualifiedClassName);
        }
    }
}
