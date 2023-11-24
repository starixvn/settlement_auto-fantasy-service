using System.Runtime.Serialization;

namespace DirectFantasy.Exceptions;

[Serializable]
public class MarkedTicketException : AutoRefundException
{
    public MarkedTicketException()
    {
        Description = "Auto refund failed: marked tickets exist.";
    }

    public MarkedTicketException(string message)
        : base(message)
    {
    }

    public MarkedTicketException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    protected MarkedTicketException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}