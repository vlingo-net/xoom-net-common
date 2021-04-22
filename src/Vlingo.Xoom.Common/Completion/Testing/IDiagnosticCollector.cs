namespace Vlingo.Xoom.Common.Completion.Testing
{
    internal interface IDiagnosticCollector
    {
        void Append(string message);
        void StartAppend(string message);
        void StopAppendStart(string message);
        void Stop();
        string Logs { get; }

    }
}