public class PlayerResourceModel
{
    public long Gold {  get; set; }

    public long Exp { get; set; }

    public uint Diamond {  get; set; }

    public long MagicStone { get; set; }

    public GuildRank PlayerGuildRank { get; set; }
}

public enum GuildRank
{
    None = 0,
    F = 1,
    E = 2,
    D = 3,
    C = 4,
    B = 5,
    A = 6,
    S = 7,
}