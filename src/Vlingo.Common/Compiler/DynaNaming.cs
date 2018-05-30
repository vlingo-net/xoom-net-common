namespace Vlingo.Common.Compiler
{
    public static class DynaNaming
    {
        public static string ClassNameFor<T>(string postfix)
        {
            var type = typeof(T);
            var className = type.Name;

            if(type.IsInterface && type.Name.StartsWith('I'))
            {
                className = className.Substring(1);
            }

            return $"{className}{postfix}";
        }

        public static string FullyQualifiedClassNameFor<T>(string postfix)
            => $"{typeof(T).Namespace}.{ClassNameFor<T>(postfix)}";
    }
}
