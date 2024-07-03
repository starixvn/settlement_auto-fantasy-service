namespace DirectFantasy.Models;

public class RemarkSettingModel
{
    private const int ConvertToBase = 2;

    private const int UnderSettlePosition = 2;

    private const int MtdHoldPosition = 4;

    private const int KeyHoldPosition = 6;

    public string SettlementStatus { get; set; }

    public RemarkSettingModel()
    {
    }

    public RemarkSettingModel(byte settlementStatus)
    {
        SettlementStatus = Convert.ToString(settlementStatus, ConvertToBase).Reverse().ToString();
    }

    public bool IsPending => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus[0].Equals('1');

    public bool IsUndecided => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > 1 && SettlementStatus[1].Equals('1');

    public bool IsUnderSettle => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > UnderSettlePosition && SettlementStatus[UnderSettlePosition].Equals('1');

    public bool IsMtdHold => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > MtdHoldPosition && SettlementStatus[MtdHoldPosition].Equals('1');

    public bool IsKeyHold => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > KeyHoldPosition && SettlementStatus[KeyHoldPosition].Equals('1');
}