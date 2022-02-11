using System.Diagnostics;
using System.Text;

namespace Vlingo.Xoom.Common.Completion.Testing;

internal class DiagnosticCollector : IDiagnosticCollector
{
    private readonly string _baseName;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private readonly StringBuilder _logs = new StringBuilder();
    private volatile object _syncLock = new object();
        
    public DiagnosticCollector(string baseName) => _baseName = baseName;

    public void Append(string message)
    {
        lock (_syncLock)
        {
            _logs.AppendLine(message);
        }
    }

    public void StartAppend(string message)
    {
        lock (_syncLock)
        {
            _stopwatch.Start();
            _logs.AppendLine($"{_baseName} : {message}");
        }
    }

    public void StopAppendStart(string message)
    {
        lock (_syncLock)
        {
            _stopwatch.Stop();
            _logs.AppendLine($"{_baseName} : {message} elapsed time '{_stopwatch.ElapsedMilliseconds}ms' on thread #{System.Threading.Thread.CurrentThread.ManagedThreadId}");
            _stopwatch.Start();   
        }
    }

    public void Stop()
    {
        lock (_syncLock)
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
            lock (_syncLock)
            {
                return _logs.ToString();
            }
        }
    }
}