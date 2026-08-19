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

    public void SetEnhanceTarget(EquipmentModel targetEquipmentModel)
    {
        var vm = GetEquipmentEnhanceViewModel();   
        
        vm.TargetEquipmentUniqueId = targetEquipmentModel.UniqueId;
        vm.ItemName = targetEquipmentModel.Name;
        vm.EnhanceLevelText = $" + {targetEquipmentModel.EnhanceLevel}";
        vm.TotalAtkText = $"공격력: {targetEquipmentModel.GetEquipmentTotalAtk()}";

        //테스트를 위해 일단은 골드만 요구하도록 로직 구현
        long cost = (targetEquipmentModel.EnhanceLevel + 1) * 10;
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
        }

        EquipmentModel targetEquipmentModel = null;

        if (targetEquipmentModel == null)
        {
            return false;
        }

        long cost = (targetEquipmentModel.EnhanceLevel + 1) * 10;

        bool isGoldUsed = NetworkManager.Instance.PlayerResourceService.RequestUseGold(cost);

        if (isGoldUsed == false)
        {
            Debug.LogWarning("골드가 부족합니다.");
            return false;
        }

        targetEquipmentModel.AddEquipmentEnhanceLevel(1);

        SetEnhanceTarget(targetEquipmentModel);

        SaveManager.Instance.SaveCurrentData();

        Debug.Log("강화 성공");

        return true;
    }




}
