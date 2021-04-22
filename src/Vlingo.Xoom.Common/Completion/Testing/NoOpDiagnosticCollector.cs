namespace Vlingo.Xoom.Common.Completion.Testing
{
    internal class NoOpDiagnosticCollector : IDiagnosticCollector
    {
        public void Append(string message)
        {
        }

        public void StartAppend(string message)
        {
        }

        public void StopAppendStart(string message)
        {
        }

        public void Stop()
        {
        }

        public string Logs => string.Empty;
    }
}