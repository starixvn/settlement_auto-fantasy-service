namespace DirectFantasy.Criteria.OtherSports;

public class ResultRefundOSInsCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string ResourceIds { get; set; }

    public short TypeId { get; set; } = 1;

    public int RowCount { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "NAP_Set_AutoRefundOS_ResultIns";
}