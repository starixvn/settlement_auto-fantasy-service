using Fanex.Data.Repository;

namespace DirectFantasy.Services.Criteria;

public class MatchCheckingCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int TypeId { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "NAP_Set_AutoRefund_MatchInfo";
}