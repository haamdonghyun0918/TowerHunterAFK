using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentInventory : MonoBehaviour
{
    public static EquipmentInventory Instance { get; private set; }
    private List<string> _ownedEquipments = new List<string>();
    public event Action<List<string>> OnEquipmentChanged;

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
            _ownedEquipments = SaveManager.Instance.CurrentSaveData.OwnedEquipments.ToList();
        }

        Debug.Log("EquipManager 호출");
        return UniTask.CompletedTask;
    }

    public void AddEquipments(string[] equipments)
    {
        if (equipments == null || equipments.Length == 0)
        {
            return;
        }

        _ownedEquipments.AddRange(equipments);
        OnEquipmentChanged?.Invoke(_ownedEquipments);
    }

    public List<string> GetOwnedEquipments()
    {
        return _ownedEquipments;
    }
}