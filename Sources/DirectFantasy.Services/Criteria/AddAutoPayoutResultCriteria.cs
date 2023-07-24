using Fanex.Data.Repository;

namespace DirectFantasy.Services.Criteria;

public class AddAutoPayoutResultCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int SourceId { get; set; }

    public int TypeId { get; set; }

    public int HomeValue { get; set; }

    public int AwayValue { get; set; }

    public int StatusId { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "NAP_Set_AutoPayoutResultIns";
}