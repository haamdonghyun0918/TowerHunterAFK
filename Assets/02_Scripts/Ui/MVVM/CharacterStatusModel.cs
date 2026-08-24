public class CharacterStatusModel
{
    public int SlotIndex {  get; set; }
    public string CharacterId { get; set; } = "";
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentSkillCost { get; set; }
    public int MaxSkillCost { get; set; }
    public bool IsActive { get; set; }
    public bool IsDead { get; set; }
    public string Name { get; set; } = "";
}
