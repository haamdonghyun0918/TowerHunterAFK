using UnityEngine;

public class EquipmentUtils : MonoBehaviour
{
    public static EquipmentUtils Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public  void AddEquipments(string baseEquipmentId)
    {
        EquipMentData equipData = GameDataManager.Instance.GetData<EquipMentData>(baseEquipmentId);

        if (equipData == null)
        {
            Debug.LogError($"[EquipmentUtils] 존재하지 않는 장비 ID입니다: {baseEquipmentId}");
            return;
        }

        SaveManager.Instance.CurrentSaveData.RecentEquipmentUid += 1;
        uint currentUid = SaveManager.Instance.CurrentSaveData.RecentEquipmentUid;

        EquipmentSaveData newEquipment = new EquipmentSaveData
        {
            UniqueId = "EQ_" + currentUid.ToString(),
            BaseId = baseEquipmentId,
            EnhanceLevel = 0
        };

        SaveManager.Instance.CurrentSaveData.OwnedEquipments.Add(newEquipment);
        SaveManager.Instance.SaveCurrentData();

        Debug.Log($"[EquipmentUtils] 장비 획득! 장비 이름: {equipData.Name}], 게임 상 장비 아이디: {newEquipment.UniqueId})");
    }
}