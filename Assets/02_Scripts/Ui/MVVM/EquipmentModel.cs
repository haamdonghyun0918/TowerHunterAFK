using System;
//일단 사용 중지
public class EquipmentModel
{
    private readonly EquipmentSaveData _saveData;
    private readonly EquipmentData _baseData;

    public EquipmentModel(EquipmentSaveData saveData, EquipmentData baseData)
    {
        _saveData = saveData;
        _baseData = baseData;
    }

    public string UniqueId => _saveData.UniqueId;
    public string BaseId => _baseData.Id;
    public string Name => _baseData.Name;
    public int EnhanceLevel => _saveData.EnhanceLevel;
    public EquipmentTier Tier => _baseData.Tier;
    public EquipmentRange Range => _baseData.Range;
    public string IconAddress => _baseData.IconAddress;
    public string Position => _baseData.Position;
    public int Rank => _baseData.Rank;
    public int BuffAtk => _baseData.BuffAtk;
    public int BuffHp => _baseData.BuffHp;
    public int BuffAtkSpeed => _baseData.BuffAtkSpeed;
    public int BuffDef => _baseData.BuffDef;
    public int Price => _baseData.Price;

    public EquipmentSlot slot
    {
        get
        {
            if(string.Equals(Position, "weapon", StringComparison.OrdinalIgnoreCase))
            {
                return EquipmentSlot.Weapon;
            }
            if(string.Equals(Position, "armor", StringComparison.OrdinalIgnoreCase))
            {
                return EquipmentSlot.Armor;
            }
            if(string.Equals(Position, "accessory", StringComparison.OrdinalIgnoreCase)|| string.Equals(Position, "accessories", StringComparison.OrdinalIgnoreCase))
            {
                return EquipmentSlot.Accessory;
            }

            return EquipmentSlot.None;
        }
    }

    public int GetEquipmentTotalAtk()
    {
        //Todo Datadriven 강화 단계 데이터셋 추가 시 교체 JU
        int enhanceBonus = EnhanceLevel * 5;
        return BuffAtk + enhanceBonus;
    }

    public int GetEquipmentTotalHp()
    {
        return BuffHp;
    }

    public int GetEquipmentTotalAtkSpeed()
    {
        return BuffAtkSpeed;
    }

    public int GetEquipmentTotalDef()
    {
        //Todo Datadriven 강화 단계 데이터셋 추가 시 교체 JU
        int enhanceBonus = EnhanceLevel * 3;

        return BuffDef + enhanceBonus;
    }

    public void AddEquipmentEnhanceLevel(int amount = 1)
    {
        if(amount <= 0)
        {
            return;
        }

        _saveData.EnhanceLevel += amount;
    }

}
