namespace Vlingo.Common.Tests.Compiler
{
    public abstract class DynaTest
    {
        protected const string ClassName = "Vlingo.Common.Tests.Compiler.TestProxy";
        protected const string Source = "namespace Vlingo.Common.Tests.Compiler { public class TestProxy : Vlingo.Common.Tests.Compiler.ITestInterface { public int Test() { return 1; } } }";
    }
}
