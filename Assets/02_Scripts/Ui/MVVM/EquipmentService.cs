using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;

public class EquipmentService
{
    private readonly Dictionary<string, EquipmentModel> _equipmentDict = new Dictionary<string, EquipmentModel>();

    public event Action<string> EquipmentChanged;
    public event Action<string> EquipmentInventoryChanged;

    public string EnhanceCostCurrentMane => "마석";
    public string DismantleRewardCurrentName => "마석";

    public bool TryGetEquipmentModel(string uniqueId, out EquipmentModel equipmentModel)
    {
        equipmentModel = null;
        if(string.IsNullOrEmpty(uniqueId))
        {
            Debug.LogError("[EquipmentService]: 장비 UniqueId가 비어있습니다.");
            return false;
        }
        if(_equipmentDict.TryGetValue(uniqueId, out equipmentModel))
        {
            return true;
        }
        if(SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[EquipmentService] SaveManager가 초기화되지 않았습니다.");
            return false;
        }
        if(SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData equipmentSaveData) == false )
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return false;
        }

        if(GameDataManager.Instance == null)
        {
            Debug.LogError("[EquipmentService] GameDataManager가 없습니다.");
            return false;
        }

        EquipmentData baseData = GameDataManager.Instance.GetData<EquipmentData>(equipmentSaveData.BaseId);

        if(baseData == null )
        {
            Debug.LogError("[EquipmentService] 장비 원본 데이터를 찾을 수 없습니다. BaseId: {saveData.BaseId}");
            return false;
        }

        equipmentModel = new EquipmentModel(equipmentSaveData, baseData);
        _equipmentDict.Add(uniqueId, equipmentModel);

        return true;
    }

    public bool ContainsEquipment(string uniqueId)
    {
        if(string.IsNullOrEmpty(uniqueId) || SaveManager.Instance == null)
        {
            return false;
        }

        return SaveManager.Instance.EquipmentDict.ContainsKey(uniqueId);
    }

    public IReadOnlyList<EquipmentModel> GetOwnedEquipmentModels()
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
            if(TryGetEquipmentModel(saveData.UniqueId, out EquipmentModel equipmentModel))
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
        //Todo DataDriven JU
        //추후 강화 레벨별 마석 데이터로 교체
        return (equipmentModel.EnhanceLevel + 1) * 10;
    }

    public bool RequestEnhance(string uniqueId)
    {
        if(TryGetEquipmentModel(uniqueId, out EquipmentModel equipmentModel) == false)
        {
            return false;
        }

        PlayerResourceService resourceService = GetPlayerResourceService();

        if(resourceService == null)
        {
            return false;
        }
        long enhanceCost = GetEnhanceCost(equipmentModel);

        if(enhanceCost <= 0)
        {
            Debug.LogError("[EquipmentService] 강화 비용이 올바르지 않습니다.");
            return false;
        }

        bool isMagicStoneUsed = resourceService.RequestUseMagicStone(enhanceCost);

        if(isMagicStoneUsed == false)
        {
            Debug.LogWarning("[EquipmentService] 마석이 부족합니다.");
            return false;
        }

        equipmentModel.AddEquipmentEnhanceLevel();

        SaveManager.Instance.SaveCurrentData();

        EquipmentChanged?.Invoke(uniqueId);

        Debug.Log($"[EquipmentService] 강화 성공: {uniqueId}, 사용 마석: {enhanceCost}");

        return true;
    }

    private PlayerResourceService GetPlayerResourceService()
    {
        if(NetworkManager.Instance == null || NetworkManager.Instance.PlayerResourceService == null)
        {
            Debug.LogError("[EquipmentService] PlayerResourceService가 없습니다.");
            return null;
        }
        
        return NetworkManager.Instance.PlayerResourceService;
    }


}