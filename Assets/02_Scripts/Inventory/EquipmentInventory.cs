using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentInventory : MonoBehaviour
{
    public static EquipmentInventory Instance { get; private set; }
    private List<EquipmentSaveData> _ownedEquipments = new List<EquipmentSaveData>();
    public event Action<List<EquipmentSaveData>> OnEquipmentChanged;

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

    public UniTask Init()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
        {
            _ownedEquipments = SaveManager.Instance.CurrentSaveData.OwnedEquipments;
            OnEquipmentChanged?.Invoke(_ownedEquipments);
        }

        Debug.Log("EquipmentInventory 호출");
        return UniTask.CompletedTask;
    }

    public List<EquipmentSaveData> GetOwnedEquipments()
    {
        return _ownedEquipments;
    }
}