using System.Runtime.Serialization;

namespace DirectFantasy.Exceptions;

[Serializable]
public class AutoRefundException : Exception
{
    public AutoRefundException()
    {
    }

    public AutoRefundException(string message)
        : base(message)
    {
    }

    public AutoRefundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    protected AutoRefundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string Description { get; set; } = string.Empty;

    public bool StopLoop { get; set; }
}