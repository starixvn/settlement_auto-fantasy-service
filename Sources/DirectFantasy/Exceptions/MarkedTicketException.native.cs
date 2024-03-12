using System.Runtime.Serialization;

namespace DirectFantasy.Exceptions;

[Serializable]
public class MarkedTicketException : AppException
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