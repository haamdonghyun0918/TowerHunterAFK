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

    public int GetEquipmentTotalAtk()
    {
        int enhanceBonus = EnhanceLevel * 5;
        return BuffAtk + enhanceBonus;
    }

    public int GetEquipmentTotalHp()
    {
        return BuffHp;
    }

    public int GetEquipmentTotalAtakSpeed()
    {
        return BuffAtkSpeed;
    }

    public int GetEquipmentTotalDef()
    {
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
