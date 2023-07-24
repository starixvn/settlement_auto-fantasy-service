using Fanex.Data;
using Fanex.Data.Repository;

namespace DirectFantasy.Services.Criteria;

public class MatchTicketInfoCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int TypeId { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => $"NAP_Set_AutoRefund_TicketInfo_{DbServer}";
}