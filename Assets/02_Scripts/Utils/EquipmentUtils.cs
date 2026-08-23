using UnityEngine;

public class EquipmentUtils
{
    public void AddEquipments(string baseEquipmentId)
    {
        EquipmentData equipData = GameDataManager.Instance.GetData<EquipmentData>(baseEquipmentId);

        if (equipData == null)
        {
            Debug.LogError($"[EquipmentUtils] 존재하지 않는 장비 ID입니다: {baseEquipmentId}");
            return;
        }

        SaveManager.Instance.CurrentSaveData.RecentEquipmentUid += 1;
        uint currentUid = SaveManager.Instance.CurrentSaveData.RecentEquipmentUid;

        EquipmentSaveData newEquipment = new EquipmentSaveData();
        newEquipment.UniqueId = "EQ_" + currentUid.ToString();
        newEquipment.BaseId = baseEquipmentId;
        newEquipment.EnhanceLevel = 0;

        SaveManager.Instance.CurrentSaveData.OwnedEquipments.Add(newEquipment);
        SaveManager.Instance.EquipmentDict[newEquipment.UniqueId] = newEquipment;
        SaveManager.Instance.SaveCurrentData();

        if (NetworkManager.Instance != null && NetworkManager.Instance.EquipmentService != null)
        {
            NetworkManager.Instance.EquipmentService.RefreshEquipmentInventory();
        }

        Debug.Log($"[EquipmentUtils] 장비 획득! 장비 이름: {equipData.Name}, 게임 상 장비 아이디: {newEquipment.UniqueId}");
    }


}