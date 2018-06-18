using System;

namespace Vlingo.Common.Compiler
{
    public static class DynaNaming
    {
        public static string ClassNameFor<T>(string postfix)
            => ClassNameFor(typeof(T), postfix);

        public static string ClassNameFor(Type type, string postfix)
        {
            var className = type.Name;

            if (type.IsInterface && type.Name.StartsWith('I'))
            {
                className = className.Substring(1);
            }
            if (type.IsGenericType)
            {
                className = className.Substring(0, className.IndexOf('`'));
            }

            return $"{className}{postfix}";
        }

        public static string FullyQualifiedClassNameFor<T>(string postfix)
            => FullyQualifiedClassNameFor(typeof(T), postfix);

        public static string FullyQualifiedClassNameFor(Type type, string postfix)
        {
            if (string.IsNullOrWhiteSpace(type.Namespace))
            {
                return ClassNameFor(type, postfix);
            }

            return $"{type.Namespace}.{ClassNameFor(type, postfix)}";
        }
    }
}
