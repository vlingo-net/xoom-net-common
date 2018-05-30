using System;
using System.Runtime.Loader;

namespace Vlingo.Common.Compiler
{
    public static class AssemblyContextHelper
    {
        public static AssemblyLoadContext SystemDefaultContext => AssemblyLoadContext.Default;

        public static AssemblyLoadContext GetAssemblyLoadContext(this Type type)
            => AssemblyLoadContext.GetLoadContext(type.Assembly);
    }
}
