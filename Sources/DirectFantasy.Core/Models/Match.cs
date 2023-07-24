using DirectFantasy.Core.Constants;

namespace DirectFantasy.Core.Models;

public class Match
{
    public int MatchId { get; set; }

    public int? HtHomeScore { get; set; }

    public int? FinalHomeScore { get; set; }

    public int? HtAwayScore { get; set; }

    public int? FinalAwayScore { get; set; }

    public string Action { get; set; } = string.Empty;

    public short AutoSetting { get; set; }

    public int TypeId { get; set; } = SettleType.FullTime;

    public int LeagueId { get; set; }
    public bool IsChecking { get; set; }

    public bool IsRefund => Action.Equals(SettleAction.Refund);
}