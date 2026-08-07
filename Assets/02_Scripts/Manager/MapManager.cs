using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    public int CurrentStage { get; private set; }

    [Header("Player Spawn & Clear Spot")]
    [SerializeField] private Transform _playerSpawnSpot;
    [SerializeField] private Transform _playerClearSpot;

    [Header("MonsterSpawnSpot")]
    [SerializeField] private Transform _monsterSpawnSpot1;
    [SerializeField] private Transform _monsterSpawnSpot2;
    [SerializeField] private Transform _monsterSpawnSpot3;

    [Header("Map Addressables")]
    [SerializeField] private SpriteRenderer _mapBackGround;
    [SerializeField] private string[] _mapAddressableKeys = { "Map1", "Map2", "Map3", "Map4", "Map5", "Map6", "Map7", "Map8" };

    public event Action<int> OnStageChanged;
    public event Action OnStageCleared;
    public event Action OnStageFailed;

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

    public async UniTask Init()
    {
        int savedStage = SaveManager.Instance.GetCurrentStage();
        await StartNewStage(savedStage);
        Debug.Log("MapManager 호출");
    }

    public async UniTask StartNewStage(int stage)
    {
        CurrentStage = stage;
        SaveManager.Instance.SaveStage(CurrentStage);

        await ChangeMapBasedOnStage(CurrentStage);
        OnStageChanged?.Invoke(CurrentStage);
    }

    public void ClearedCurrentStage()
    {
        OnStageCleared?.Invoke();
        StartNewStage(CurrentStage + 1).Forget();
    }

    public void FailedCurrentStage()
    {
        OnStageFailed?.Invoke();
        int rollBackStage = ((CurrentStage - 1) / 10) * 10 + 1;
        StartNewStage(rollBackStage).Forget();
    }

    private async UniTask ChangeMapBasedOnStage(int currentStage)
    {
        int mapIndex = ((currentStage - 1) % (_mapAddressableKeys.Length * 10)) / 10;

        if (mapIndex >= _mapAddressableKeys.Length)
        {
            Debug.LogError("스테이지 단계에 맞는 맵이 존재 X");
        }

        await LoadMapSprite(_mapAddressableKeys[mapIndex]);
    }

    private async UniTask LoadMapSprite(string mapKey)
    {
        try
        {
            Sprite loadedSprite = await Addressables.LoadAssetAsync<Sprite>(mapKey);

            if (loadedSprite != null)
            {
                _mapBackGround.sprite = loadedSprite;
                Debug.Log("맵 이미지 불러오기 성공");
            }
        }

        catch
        {
            Debug.LogError("맵 이미지를 불러기 실패");
        }
    }
}