using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Settlement.ScaleOut.Constants;

namespace DirectFantasy.Criteria.OtherSports;

public class SettleCriteria : NonQueryCommand
{
    public int AdminId { get; set; }

    public int MatchId { get; set; }

    public string SelectedResources { get; set; }

    public string Action { get; set; } = "CAL";

    public bool IsLeader { get; } = true;

    public int? AutoType { get; set; }

    public bool IsAuto { get; } = true;

    public string AutoSourceInfo { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public string BroadcastSessionId { get; } = Guid.NewGuid().ToString();

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    [SpParam(Ignored = true)]
    public string SpName { get; } = "NAP_Admin_Settlement_Profile_Push";

    public override string GetSettingKey() => $"NAP_Admin_Settlement_Profile_Push_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}