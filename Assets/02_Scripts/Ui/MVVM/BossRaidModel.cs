public class BossRaidModel
{
    public const int MaxPartySize = 5;
    public BossData SelectedBoss { get; set; }
    public bool IsRaidInProgress { get; set; }
    public string[] PartyUids { get; } = new string[MaxPartySize] { "", "", "", "", "" };
}