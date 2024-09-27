using System.Runtime.Serialization;

namespace DirectFantasy.Exceptions;

[Serializable]
public class AppException : Exception
{
    public AppException()
    {
    }

    public AppException(string message)
        : base(message)
    {
    }

    public AppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    protected AppException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string Description { get; set; } = string.Empty;
}