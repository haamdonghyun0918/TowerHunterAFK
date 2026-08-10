using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public SaveData CurrentSaveData { get; private set; }
    private const string SaveFileName = "GameSaveData.json";

    public event Action<long> OnGoldChanged;
    public event Action OnNotEnoughGold;

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
        CurrentSaveData = LoadOrCreateData();
        Debug.Log("SaveManager 호출");
        return UniTask.CompletedTask;
    }

    private SaveData LoadOrCreateData()
    {
        if (HasSaveFile())
        {
            return LoadFromFile();
        }

        SaveData newData = new SaveData();
        SaveToFile(newData);
        return newData;
    }
    private string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private bool HasSaveFile()
    {
        return File.Exists(GetPath());
    }

    private void SaveToFile(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(), json);
        Debug.Log($"[SaveManager] 세이브 완료: {GetPath()}");
    }

    private SaveData LoadFromFile()
    {
        string json = File.ReadAllText(GetPath());
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"[SaveManager] 세이브 로드 완료: {GetPath()}");
        return data;
    }

    public void SaveStage(int stage)
    {
        CurrentSaveData.CurrentStage = stage;
        SaveToFile(CurrentSaveData);
    }

    public int GetCurrentStage()
    {
        return CurrentSaveData.CurrentStage;
    }

    public void SaveGold(long gold)
    {
        CurrentSaveData.Gold = gold;
        SaveToFile(CurrentSaveData);
        OnGoldChanged?.Invoke(CurrentSaveData.Gold);
    }

    public void SaveItem(string[] items)
    {
        if (items == null || items.Length == 0)
        {
            return;
        }

        //TODO: 인벤토리(창고) 시스템 구현시 실제 그 위치에 저장되게 하기
        CurrentSaveData.InventoryItems.AddRange(items);
        SaveToFile(CurrentSaveData);
        Debug.Log("[SaveManager] 아이템을 획득하여 저장하였습니다.");
    }
}