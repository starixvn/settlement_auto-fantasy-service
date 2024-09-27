namespace DirectFantasy.Exceptions;

public class RelatedMatchesInsufficientException : AppException
{
    public RelatedMatchesInsufficientException()
    {
        Description = "Failed: Parent matches is insufficient";
    }
}