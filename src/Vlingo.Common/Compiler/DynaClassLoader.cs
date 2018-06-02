using System;
using System.IO;
using System.Reflection;
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
                var loadedAssembly = context.LoadFromStream(stream);
                return loadedAssembly.GetType(fullyQualifiedClassName);
            }
        }

        internal Type AddDynaClass(string fullyQualifiedClassName, string dllPath)
        {
            var loadedAssembly = context.LoadFromAssemblyPath(dllPath);
            return loadedAssembly.GetType(fullyQualifiedClassName);
        }

        public Type LoadClass(string fullyQualifiedClassName)
        {
            var assemblies = AppDomain.CurrentDomain?.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                var type = assembly.GetType(fullyQualifiedClassName);
                if(type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
