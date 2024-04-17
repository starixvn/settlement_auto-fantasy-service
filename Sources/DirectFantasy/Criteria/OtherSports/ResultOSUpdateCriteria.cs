using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectFantasy.Criteria.OtherSports;

public class ResultOSUpdateCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string SelectedResources { get; set; }

    public int StatusId { get; set; }

    public string Remark { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "NAP_Set_AutoPayoutResultOS_Update";
}