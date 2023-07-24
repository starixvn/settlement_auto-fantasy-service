using System.Runtime.Serialization;

namespace DirectFantasy.Core.Exceptions;

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

    protected MarkedTicketException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}