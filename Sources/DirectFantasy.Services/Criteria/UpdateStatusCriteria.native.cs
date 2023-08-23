using Fanex.Data.Repository;

namespace DirectFantasy.Services.Criteria;

public class UpdateStatusCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int StatusId { get; set; }

    public int TypeId { get; set; }

    public string Remark { get; set; }

    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResultUpd";

    public override bool IsValid() => MatchId > 0 || StatusId > 0;
}