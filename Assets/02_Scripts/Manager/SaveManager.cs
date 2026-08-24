using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public SaveData CurrentSaveData { get; private set; }
    private const string SaveFileName = "GameSaveData.json";

    public Dictionary<string, CharacterSaveData> CharacterDict = new Dictionary<string, CharacterSaveData>();
    public Dictionary<string, EquipmentSaveData> EquipmentDict = new Dictionary<string, EquipmentSaveData>();

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

        CharacterDict.Clear();
        foreach (var character in CurrentSaveData.OwnedCharacters)
        {
            CharacterDict[character.UniqueId] = character;
        }

        EquipmentDict.Clear();
        foreach (var equip in CurrentSaveData.OwnedEquipments)
        {
            EquipmentDict[equip.UniqueId] = equip;
        }

        Debug.Log("SaveManager 호출 및 캐싱 완료");
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
        if(data == null)
        {
            Debug.LogError($"[SaveMangaer]: 저장할 SaveData가 없습니다.");
            return;
        }
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
        if(CurrentSaveData == null)
        {
            Debug.LogError("[SaveManager] Init이 먼저 호출되어야 합니다.");
            return 1;
        }

        return CurrentSaveData.CurrentStage;
    }

    public void UpdateMaxClearedStage(int clearedStage)
    {
        if (clearedStage > CurrentSaveData.MaxClearedStage)
        {
            CurrentSaveData.MaxClearedStage = clearedStage;

            int newLevel = Mathf.Max(1, CurrentSaveData.MaxClearedStage);

            if (newLevel > CurrentSaveData.PlayerLevel)
            {
                CurrentSaveData.PlayerLevel = newLevel;
                Debug.Log($"[SaveManager] 레벨 업! 현재 플레이어 레벨: {CurrentSaveData.PlayerLevel}");
            }

            SaveToFile(CurrentSaveData);
        }
    }

    public int GetMaxClearedStage()
    {
        if( CurrentSaveData == null)
        {
            Debug.LogError("[SaveManager] Init이 먼저 호출되어야 합니다.");
            return 0;
        }

        return CurrentSaveData.MaxClearedStage;
    }

    public void SaveGold(long gold)
    {
        CurrentSaveData.Gold = gold;
        SaveToFile(CurrentSaveData);
    }

    public void SaveExp(long exp)
    {
        CurrentSaveData.Exp = exp;
        SaveToFile(CurrentSaveData);
    }

    public void SaveDiamond(uint diamond)
    {
        CurrentSaveData.Diamond = diamond;
        SaveToFile(CurrentSaveData);
    }

    public void SaveMagicStone(long magicStone)
    {
        CurrentSaveData.MagicStone = magicStone;
        SaveToFile(CurrentSaveData);
    }

    public void SaveEquipments(List<EquipmentSaveData> equipments)
    {
        if (equipments == null)
        {
            return;
        }

        CurrentSaveData.OwnedEquipments = equipments;
        SaveToFile(CurrentSaveData);
        Debug.Log("[SaveManager] 장비를 획득하여 저장하였습니다.");
    }

    public void SaveCharacters(List<CharacterSaveData> characters)
    {
        if (characters == null)
        {
            return;
        }

        CurrentSaveData.OwnedCharacters = characters;
        SaveToFile(CurrentSaveData);
        Debug.Log("[SaveManager] 헌터를 획득하여 저장하였습니다.");
    }

    public void SavePlayerLevel(int level)
    {
        CurrentSaveData.PlayerLevel = level;
        SaveToFile(CurrentSaveData);
    }

    public int GetPlayerLevel()
    {
        return CurrentSaveData.PlayerLevel;
    }

    public void SaveExpeditionStart(string expeditionId, string startTime)
    {
        CurrentSaveData.OngoingExpeditionId = expeditionId;
        CurrentSaveData.ExpeditionStartTime = startTime;
        SaveToFile(CurrentSaveData);
        Debug.Log($"[SaveManager] 원정 시스템 저장 완료- ID: {expeditionId}, 시작시간: {startTime}");
    }

    public void ClearExpedition()
    {
        CurrentSaveData.OngoingExpeditionId = "";
        CurrentSaveData.ExpeditionStartTime = "";
        SaveToFile(CurrentSaveData);
        Debug.Log("[SaveManager] 원정 상태 초기화");
    }

    public string GetOngoingExpeditionId()
    {
        return CurrentSaveData.OngoingExpeditionId;
    }

    public string GetExpeditionStartTime()
    {
        return CurrentSaveData.ExpeditionStartTime;
    }

    public void SaveCurrentData()
    {
        SaveToFile(CurrentSaveData);
    }
}