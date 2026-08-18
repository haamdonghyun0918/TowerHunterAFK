using UnityEditor.Overlays;
using UnityEngine;

public class EquipmentModel
{
    public EquipmentSaveData _saveData { get; private set; }
    public EquipMentData _baseData { get; private set; }

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

    //이하 단순 값 수정 메서드.
    //_saveData, _baseData를 여기저기서 수정하지 않고 EquipmentModel를 통해서만 수정하게 해야한다 (그렇지 않으면 기껏 만들어놓은 이유가 없어짐)
    //모델에 이것들 두는게 맞는가 고민중.

    public int GetTotalAtk()
    {
        int enhanceBonus = _saveData.EnhanceLevel * 5;
        return _baseData.BuffAtk + enhanceBonus;
    }

    public int GetTotalDef()
    {
        int enhanceBonus = _saveData.EnhanceLevel * 3;
        return _baseData.BuffDef + enhanceBonus;
    }

    public void AddEnhanceLevel(int amount = 1)
    {
        _saveData.EnhanceLevel += amount;
    }
}
