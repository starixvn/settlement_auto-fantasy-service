namespace DirectFantasy.Models;

public class FantasyMatchDetail
{
    public string FantasyHomeTeam { get; set; }

    public string FantasyAwayTeam { get; set; }
}

public class FantasyMatch
{
    public FantasyMatchDetail FantasyMatchDetail { get; set; }

    public List<RelatedMatch> RelatedMatches { get; set; }
}

public class RelatedMatch
{
    public int MatchId { get; set; }
    public string MatchCode { get; set; }

    public int HomeId { get; set; }

    public int AwayId { get; set; }

    public DateTime KickOffTime { get; set; }

    public RemarkSettingModel Remark { get; set; }

    public List<ResultDetail> Results { get; set; }

    public byte SettlementStatus { get; set; }
    public string BetStatus { get; set; }
    public string EventStatus { get; set; }
    public string NewEventStatus { get; set; }
    public bool IsDeleted { get; set; }
}

public class FantasyRelatedMatch
{
    public int MatchId { get; set; }

    public string MatchCode { get; set; }

    public DateTime KickOffTime { get; set; }

    public int HomeId { get; set; }

    public int AwayId { get; set; }

    public string ResourceId { get; set; }
    public string HomeValue { get; set; }
    public string AwayValue { get; set; }
    public byte ResultStatusId { get; set; }
    public byte SettlementStatusId { get; set; }

    public byte SettlementStatus { get; set; }

    public string BetStatus { get; set; }

    public string EventStatus { get; set; }

    public RemarkSettingModel Remark => new(SettlementStatus);

    public bool IsDeleted { get; set; }
}