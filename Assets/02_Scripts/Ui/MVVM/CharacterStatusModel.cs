public class CharacterStatusModel
{
    public int SlotIndex {  get; set; }
    public int CharacterId { get; set; } = "";
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentSkillCost { get; set; }
    public int MaxSkillCost { get; set; }
    public bool IsActive { get; set; }
    public bool isDead { get; set; }
}
