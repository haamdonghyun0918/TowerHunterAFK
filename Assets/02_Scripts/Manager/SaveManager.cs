using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public SaveData CurrentSaveData { get; private set; }
    private const string SaveFileName = "GameSaveData.json";

    public event Action<int> OnGoldChanged;
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

    public void Init()
    {
        CurrentSaveData = LoadOrCreateData();
        Debug.Log("SaveManager 호출");
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

    public void SaveGold(int gold)
    {
        CurrentSaveData.Gold = gold;
        SaveToFile(CurrentSaveData);
        OnGoldChanged?.Invoke(CurrentSaveData.Gold);
    }

    public void AddGold(int amount)
    {
        CurrentSaveData.Gold += amount;
        SaveToFile(CurrentSaveData);
        OnGoldChanged?.Invoke(CurrentSaveData.Gold);
    }

    public int GetGold()
    {
        return CurrentSaveData.Gold;
    }

    public bool UseGold(int amount)
    {
        if (CurrentSaveData.Gold >= amount)
        {
            CurrentSaveData.Gold -= amount;
            SaveToFile(CurrentSaveData);
            OnGoldChanged?.Invoke(CurrentSaveData.Gold);

            return true;
        }

        else
        {
            OnNotEnoughGold?.Invoke();
            return false;
        }
    }
}