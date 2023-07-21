using Fanex.Data.Repository;

namespace DirectPayout.Fantasy.Criteria;

public class GetFantasyMatchesCriteria : CriteriaBase
{
    public string Action { get; set; } = "CAL";

    public override string GetSettingKey() => "NAP_Set_AutoPayout_Fantasy_ScheduleSel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(Action);
}