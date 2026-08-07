using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    [Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> item;
    }

    private Dictionary<string, object> _dataList = new Dictionary<string, object>();

    private Dictionary<string, T> LoadJsonData<T>(string tableName) where T : GameDataBase
    {
        string resourcePath = $"JsonOutput/{tableName}";
        
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if(textAsset == null)
        {
            Debug.LogError($"GameDataManager: 리소스를 찾을 수 없습니다. Resource/{resourcePath}");
            return new Dictionary<string, T> ();
        }

        try
        {
            string JsonString = textAsset.text;

            string wrappedJson = "{\"Item\":" +  JsonString +"}";
            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if(wrapper != null && wrapper.item != null)
            {
                Debug.Log($"{typeof(T).Name} 데이터를 {wrapper.items.Count}개 로드했습니다.");
                return wrapper.item.ToDictionary(item => item.Id.ToString());
            }
        }
        catch(Exception ex)
        {
            Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T> ();
    }

    public void LoadData<T>() where T : GameDataBase
    {
        string dataName = typeof(T).Name;
        if(_dataList.ContainsKey(dataName) == false)
        {
            _dataList.Add(dataName, new Dictionary<string, T>());
        }
        _dataList[dataName] = LoadJsonData<T>(dataName);
    }

    public T GetData<T>(string id) where T : GameDataBase
    {
        string type = typeof(T).Name;
        object dictObject = null;
        if(_dataList.TryGetValue(type, out dictObject))
        {
            var dict = dictObject as Dictionary<string, T>;
            return dict[id];
        }
        return null;
    }

    public List<string> GetAllDataId<T>() where T : GameDataBase
    {
        string type = typeof(T).Name;
        object dictObject = null;
        if (_dataList.TryGetValue(type, out dictObject))
        {
            var dict = dictObject as Dictionary<string, T>;
            return dict.Keys.ToList();
        }
        return null;
    }
    
    public List<T> GetAllData<T>() where T : GameDataBase
    {
        string type = typeof(T).Name;
        object dictObject = null;
        if(_dataList.TryGetValue(type,out dictObject))
        {
            var dict = dictObject as Dictionary<string, T>;
            return dict.Values.ToList();
        }
        return null;
    }
}
