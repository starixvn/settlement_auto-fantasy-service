namespace DirectFantasy.Core.Models;

public class Result
{
    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = string.Empty;

    public bool IsSuccessful => string.IsNullOrWhiteSpace(ErrMsg) && ErrCode == 0;
}