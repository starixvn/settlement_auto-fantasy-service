using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectFantasy.Criteria.OtherSports;

public class OSMatchesSelCriteria : CriteriaBase
{
    public override string GetSettingKey() => "NAP_Set_AutoPayout_FantasyOS_ScheduleSel";

    public override bool IsValid() => true;
}