namespace DirectFantasy.Core.Models;

public class AppSettingsKey
{
    public int RetryCount { get; set; }

    public int RetryInterval { get; set; }

    public string EnvironmentName { get; set; }

    public List<string> SiteDbServer { get; set; }
}