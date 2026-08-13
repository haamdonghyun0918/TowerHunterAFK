using System;
using System.Collections.Generic;

public class EquipmentInventory
{
    private List<EquipmentSaveData> _ownedEquipments = new List<EquipmentSaveData>();
    public event Action<List<EquipmentSaveData>> OnEquipmentChanged;

    public void Init()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
        {
            _ownedEquipments = SaveManager.Instance.CurrentSaveData.OwnedEquipments;

            if (OnEquipmentChanged != null)
            {
                OnEquipmentChanged(_ownedEquipments);
            }
        }
    }

    public List<EquipmentSaveData> GetOwnedEquipments()
    {
        return _ownedEquipments;
    }


}