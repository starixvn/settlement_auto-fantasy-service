namespace DirectFantasy.Criteria.OtherSports;

public class RelatedMatchesSelCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey() => "NAP_Set_FantasyMatchOSSel";

    public override bool IsValid() => MatchId > 0;
}