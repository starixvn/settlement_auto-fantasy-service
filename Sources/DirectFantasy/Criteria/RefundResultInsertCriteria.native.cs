namespace DirectFantasy.Criteria;

public class RefundResultInsertCriteria : NonQueryCommand
{
    public int ParentId { get; init; }
    public int TypeId { get; init; }

    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResultIns";

    public override bool IsValid() => ParentId > 0;
}