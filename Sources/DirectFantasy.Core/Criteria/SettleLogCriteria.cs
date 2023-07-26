using DirectFantasy.Core.Constants;
using Fanex.Data;
using Fanex.Data.Repository;

namespace DirectFantasy.Core.Criteria;

#nullable disable

public class SettleLogCriteria : NonQueryCommand
{
    public int UserId { get; set; }

    public int MatchId { get; set; }

    [SpParam(Ignored = true)]
    public int TypeId { get; set; }

    public int ActionId => TypeId == SettleType.FullTime ? SettleActionId.FullTypeAfterSettleFail : SettleActionId.HalfTypeAfterSettleFail;

    public int? FinalHomeScore { get; set; }

    public int? FinalAwayScore { get; set; }

    public int? HtHomeScore { get; set; }

    public int? HtAwayScore { get; set; }

    public string Remark { get; set; }

    public string GoalRecord { get; set; }

    public string Ip { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "WriteLog_UserLog_Settle";
}