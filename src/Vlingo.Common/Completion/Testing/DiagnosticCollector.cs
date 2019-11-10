using System.Diagnostics;
using System.Text;

namespace Vlingo.Common.Completion.Testing
{
    internal class DiagnosticCollector : IDiagnosticCollector
    {
        private readonly string _baseName;
        private Stopwatch _stopwatch = new Stopwatch();
        private StringBuilder _logs = new StringBuilder();
        private volatile object syncLock = new object();
        
        public DiagnosticCollector(string baseName) => _baseName = baseName;

        public void Append(string message) => _logs.AppendLine(message);

        public void StartAppend(string message)
        {
            lock (syncLock)
            {
                _stopwatch.Start();
                _logs.AppendLine($"{_baseName} : {message}");
            }
        }

        public void StopAppendStart(string message)
        {
            lock (syncLock)
            {
                _stopwatch.Stop();
                _logs.AppendLine($"{_baseName} : {message} elapsed time '{_stopwatch.ElapsedMilliseconds}ms' on thread #{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                _stopwatch.Start();   
            }
        }

        public void Stop()
        {
            lock (syncLock)
            {
                _stopwatch.Stop();
                _logs.AppendLine($"{_baseName} : Stopped with total elapsed time '{_stopwatch.ElapsedMilliseconds}ms' on thread #{System.Threading.Thread.CurrentThread.ManagedThreadId}");   
            }
        }

        public string Logs
        {
            get
            {
                Stop();
                return _logs.ToString();
            }
        }
    }
}