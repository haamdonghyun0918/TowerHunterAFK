using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public SaveData CurrentSaveData { get; private set; }
    private const string SaveFileName = "GameSaveData.json";

    //방치형은 재화 단위가 커질 수 있어 long으로 데이터 타입 변경 - 이에 맞춰 밑에도 다 반영함.
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

    //골드 데이터 타입 변경
    public void SaveGold(long gold)
    {
        CurrentSaveData.Gold = gold;
        SaveToFile(CurrentSaveData);
        OnGoldChanged?.Invoke(CurrentSaveData.Gold);
    }
    
    //골드 데이터 타입 변경
    //MVVM구조에 맞게 삭제해야 할지도?
    public void AddGold(long amount)
    {
        CurrentSaveData.Gold += amount;
        SaveToFile(CurrentSaveData);
        OnGoldChanged?.Invoke(CurrentSaveData.Gold);
    }

    public long GetGold()
    {
        return CurrentSaveData.Gold;
    }

    //골드 데이터 타입 변경
    public bool UseGold(long amount)
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