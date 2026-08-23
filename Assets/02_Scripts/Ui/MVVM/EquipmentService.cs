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

    
}