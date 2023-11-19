namespace WhoIsLive.Lib.Events;

public class ProcessErrorEventArgs : EventArgs
{
    public ProcessErrorEventArgs(string source, string message)
    {
        Source = source;
        Message = message;
    }

    public string Source { get; }
    public string Message { get;}
}