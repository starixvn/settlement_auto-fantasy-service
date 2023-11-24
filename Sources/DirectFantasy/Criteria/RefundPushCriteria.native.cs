using DirectFantasy.Constants;

namespace DirectFantasy.Criteria;

public class RefundPushCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int AdminId { get; init; }

    public string Action { get; } = SettleAction.Refund;

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public int IsAuto { get; } = 1;

    public string BroadcastSessionId { get; } = Guid.NewGuid().ToString();

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => $"NAP_Admin_Settlement_Refund_1_Push_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}