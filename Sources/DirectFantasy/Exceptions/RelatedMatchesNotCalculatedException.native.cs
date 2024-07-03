namespace DirectFantasy.Exceptions;

public class RelatedMatchesNotCalculatedException : AppException
{
    public RelatedMatchesNotCalculatedException()
    {
        Description = "Failed: Parent match is not completed";
    }
}