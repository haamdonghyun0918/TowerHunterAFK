using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SaveFileName = "GameSaveData.json";
    public SaveData CurrentSaveData { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentSaveData = LoadOrCreateData();
        }

        else
        {
            Destroy(gameObject);
        }
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
    }

    public void AddGold(int amount)
    {
        CurrentSaveData.Gold += amount;
        SaveToFile(CurrentSaveData);
    }

    public int GetGold()
    {
        return CurrentSaveData.Gold;
    }
}