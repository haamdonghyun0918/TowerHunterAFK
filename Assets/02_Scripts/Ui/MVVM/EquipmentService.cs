using UnityEngine;

public class EquipmentService
{
    private EquipmentEnhanceViewModel _equipmentEnhanceViewModel;

    public EquipmentEnhanceViewModel GetEquipmentEnhanceViewModel()
    {
        if (_equipmentEnhanceViewModel == null)
        {
            _equipmentEnhanceViewModel = new EquipmentEnhanceViewModel();
        }
        return _equipmentEnhanceViewModel;
    }

    public void SetEnhanceTarget(string uniqueId)
    {
        if (SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false)
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return;
        }

        EquipMentData baseData = GameDataManager.Instance.GetData<EquipMentData>(saveData.BaseId);

        if (baseData == null)
        {
            Debug.LogError($"[EquipmentService] 해당 baseData의 장비를 찾을 수 없습니다: {baseData}");
            return;
        }

        var vm = GetEquipmentEnhanceViewModel();

        vm.TargetEquipmentUniqueId = uniqueId;
        vm.ItemName = baseData.Name;
        vm.EnhanceLevel = saveData.EnhanceLevel;

        int totalAtk = baseData.BuffAtk + (saveData.EnhanceLevel * 5);
        vm.TotalAtkText = $"공격력: {totalAtk}";

        int totalDef = baseData.BuffDef + (saveData.EnhanceLevel * 3);
        vm.TotalDefText = $"방어력: {totalDef}";

        //테스트를 위해 일단은 골드만 요구하도록 로직 구현
        long cost = (saveData.EnhanceLevel + 1) * 10;
        vm.CostText = $"{cost} Gold";
    }

    public bool RequestEnhance(string uniqueId)
    {
        if (SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false)
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return false;
        }

        EquipMentData baseData = GameDataManager.Instance.GetData<EquipMentData>(saveData.BaseId);
        if (baseData == null)
        {
            Debug.LogError($"[EquipmentService] 해당 장비의 BaseData를 찾을 수 없습니다: {saveData.BaseId}");
            return false;
        }

        long cost = (saveData.EnhanceLevel + 1) * 10;
        bool isGoldUsed = NetworkManager.Instance.PlayerResourceService.RequestUseGold(cost);

        if (isGoldUsed == false)
        {
            Debug.LogWarning("골드가 부족합니다.");
            return false;
        }

        saveData.EnhanceLevel += 1;

        SetEnhanceTarget(uniqueId);

        SaveManager.Instance.SaveCurrentData();

        Debug.Log("강화 성공");

        return true;
    }




}
