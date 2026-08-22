using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentService
{
    private readonly Dictionary<string, EquipmentModel> _equipmentModelDict = new Dictionary<string, EquipmentModel>();

    public event Action<string> EquipmentChanged;
    public event Action EquipmentInventoryChanged;

    public string DismantleRewardCurrencyName => "Gold";

    public bool TryGetEuipmentModel(string uniqueId, out EquipmentModel equipmentModel)
    {
        equipmentModel = null;

        if(string.IsNullOrEmpty(uniqueId))
        {
            Debug.LogError($"[EquipmentService]: 장비 uniqueId가 비어있습니다.");
            return false;
        }

        if(_equipmentModelDict.TryGetValue(uniqueId, out equipmentModel))
        {
            return true;
        }

        if(SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[EquipmentService] SaveManager가 초기화되지 않았습니다.");
            return false;
        }

        if(SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false )
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return false;
        }

        if(GameDataManager.Instance == null)
        {
            Debug.LogError("[EquipmentService]: GameDataManager가 없습니다.");

            return false;
        }

        EquipmentData baseData = GameDataManager.Instance.GetData<EquipmentData>(saveData.BaseId);

        if(baseData == null )
        {
            Debug.LogError($"[EquipmentService] 장비 원본 데이터를 찾을 수 없습니다. BaseId: {saveData.BaseId}");

            return false;
        }

        equipmentModel = new EquipmentModel(saveData, baseData);

        _equipmentModelDict.Add(uniqueId, equipmentModel);

        return true;
    }

    public bool ContainsEquipment(string uniqueId)
    {
        if (string.IsNullOrEmpty(uniqueId) || SaveManager.Instance == null)
        {
            return false;
        }

        return SaveManager.Instance.EquipmentDict.ContainsKey(uniqueId);
    }

    public IReadOnlyList<EquipmentModel> GetOwenedEquipmentModels()
    {
        List<EquipmentModel> equipmentModels = new List<EquipmentModel>();

        if(SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            return equipmentModels;
        }

        List<EquipmentSaveData> ownedEquipments = SaveManager.Instance.CurrentSaveData.OwnedEquipments;

        if(ownedEquipments == null)
        {
            return equipmentModels;
        }

        for(int i = 0; i < ownedEquipments.Count; i++)
        {
            EquipmentSaveData saveData = ownedEquipments[i];

            if(saveData == null)
            {
                continue;
            }

            if(TryGetEuipmentModel(saveData.UniqueId, out EquipmentModel equipmentModel))
            {
                equipmentModels.Add(equipmentModel);
            }
        }

        return equipmentModels;
    }

    public long GetEnhanceCost(EquipmentModel equipmentModel)
    {
        if(equipmentModel == null)
        {
            return 0;
        }

        //ToDo_DataDriven - JU
        //추후 강화 레벨별 비용 데이터로 교체한다.
        //현재는 하드코딩
        return (equipmentModel.EnhanceLevel + 1) * 10;
    }

    public bool RequestEnhance(string uniqueId)
    {
        if(TryGetEuipmentModel(uniqueId, out EquipmentModel equipmentModel) == false)
        {
            return false;
        }

        if(NetworkManager.Instance == null || NetworkManager.Instance.PlayerResourceService == null)
        {
            Debug.LogError("[EquipmentService] PlayerResourceService가 없습니다.");

            return false;
        }

        long enhanceCost = GetEnhanceCost(equipmentModel);

        if(enhanceCost <= 0)
        {
            Debug.LogError("[EquipmentService] 강화 비용이 올바르지 않습니다.");
            return false;
        }
        //ToDo - 마석으로 강화하는 로직을 짤 대 수정할 것. JU
        bool isGoldUsed = NetworkManager.Instance.PlayerResourceService.RequestUseGold(enhanceCost);
    }
}