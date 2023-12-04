namespace DirectFantasy.Criteria;

public class GetFantasyMatchesCriteria : CriteriaBase
{
    public override string GetSettingKey() => "NAP_Set_AutoPayout_Fantasy_ScheduleSel";

    public override bool IsValid() => true;
}