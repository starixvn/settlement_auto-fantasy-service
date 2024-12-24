namespace DirectFantasy.Constants;

public static class ResourceId
{
    public const string Halftime = "01";

    public const string Final = "00";

    public const string Ot = "OT";

    public static readonly string[] Basketball = ["Q1", "Q2", "Q3", "Q4", "00", "01"];
}

public static class SettleType
{
    public const int Halftime = 1;

    public const int FullTime = 2;
}

public static class CacheKey
{
    public const string LeagueId = "Settlement-AutoPayout-AutoInstant-LeagueId";
    public const string OsLeagueId = "Settlement-AutoPayoutOtherSport-LeagueId";
}

public static class Environment
{
    public const string Pro = "PRO";

    public const string Uat = "UAT";
}

public static class MatchStatus
{
    public const int Waiting = 1;
    public const int Completed = 2;
    public const int Failed = 3;
}

public static class SettleAction
{
    public const string Refund = "REFUND";
}

public static class SettleActionId
{
    public const int CalcAll = 100;

    public const int RefundSingleAction = 81022;
}

public static class Source
{
    public const int InfoSite = 100;
}

public static class SpName
{
    public const string PayoutPush = "NAP_Admin_Settlement_1_AutoPayout_Push";
    public const string RefundPush = "NAP_Admin_Settlement_Refund_1_Push";
}

public static class EventStatus
{
    public const string Running = "running";
    public const string Completed = "completed";
}