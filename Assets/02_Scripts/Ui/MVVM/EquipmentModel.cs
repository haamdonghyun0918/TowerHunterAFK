using System;

public class EquipmentModel
{
    private EquipmentSaveData _saveData;
    private EquipMentData _baseData;

    public EquipmentModel(EquipmentSaveData saveData, EquipMentData baseData)
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

    public int GetEquipmentTotalAtk()
    {
        int enhanceBonus = _saveData.EnhanceLevel * 5;
        return _baseData.BuffAtk + enhanceBonus;
    }

    public int GetEquipmentTotalDef()
    {
        int enhanceBonus = _saveData.EnhanceLevel * 3;
        return _baseData.BuffDef + enhanceBonus;
    }

    public void AddEquipmentEnhanceLevel(int amount = 1)
    {
        _saveData.EnhanceLevel += amount;
    }
}
