namespace DirectFantasy.Exceptions;

[Serializable]
public class HoldStatusException : AppException
{
    public HoldStatusException()
    {
        Description = "Failed: Cannot execute auto-settle process due to match is on hold.";
    }
}