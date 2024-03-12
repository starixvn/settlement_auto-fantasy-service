using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectFantasy.Criteria.OtherSports;

public class RelatedMatchesSelCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey() => "NAP_Set_FantasyMatchOSSel";

    public override bool IsValid() => MatchId > 0;
}