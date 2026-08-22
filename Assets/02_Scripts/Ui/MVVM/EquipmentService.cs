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
}