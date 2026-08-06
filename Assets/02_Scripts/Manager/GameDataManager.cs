using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;

        LoadAll();  // => 추후 GameUtil에서 불러오도록 변경
    }

    [System.Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> items;
    }

    // -------------------------------------------------------------------

    public Dictionary<string, CharacterData> CharacterDataList { get; private set; } = new Dictionary<string, CharacterData>();
    public Dictionary<string, BaseStatData> BaseStatDataList { get; private set; } = new Dictionary<string, BaseStatData>();

    private Dictionary<string, T> LoadData<T>(string tableName) where T : GameDataBase
    {
        string resourcePath = $"JsonOutput/{tableName}";

        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError($"[Error] 리소스를 찾을 수 없습니다: Resources/{resourcePath}");
            return new Dictionary<string, T>();
        }

        try
        {
            string jsonString = textAsset.text;

            string wrappedJson = "{\"items\":" + jsonString + "}";
            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper != null && wrapper.items != null)
            {
                Debug.Log($"{typeof(T).Name} 데이터를 {wrapper.items.Count}개 로드했습니다.");
                // ToDictionary를 사용하려면 각 클래스(T)에 Id 필드가 있어야 합니다.
                return wrapper.items.ToDictionary(item => item.Id.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>();
    }

    public void LoadAll()
    {
        CharacterDataList = LoadData<CharacterData>("Character");

        BaseStatDataList = LoadData<BaseStatData>("BaseStat");
    }

    // [사용부] ===========================================================

    public CharacterData GetCharacterData(string id)
    {
        if (CharacterDataList == null || string.IsNullOrEmpty(id)) return null;

        return CharacterDataList.TryGetValue(id, out var item) ? item : null;
    }

    public BaseStatData GetBaseStatData(string id)
    {
        if (BaseStatDataList == null || string.IsNullOrEmpty(id)) return null;

        return BaseStatDataList.TryGetValue(id, out var item) ? item : null;
    }
}
