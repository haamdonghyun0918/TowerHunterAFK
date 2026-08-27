using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    public int CurrentStage
    {
        get
        {
            if(NetworkManager.Instance == null || NetworkManager.Instance.StageService == null)
            {
                return 1;
            }
            return NetworkManager.Instance.StageService.CurrentStage;
        }
    }

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

    [Header("Boss Raid")]
    [SerializeField] private Transform _playerSpawnSpotForBoss;
    [SerializeField] private Transform _monsterSpawnSpotForBoss;
    [SerializeField] private SpriteRenderer _mapBackGroundForBoss;
    [SerializeField] private string _mapAddressableKeyForBoss = "Map1";

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

    private void OnEnable()
    {
        MainUi.OnBossRaidStart += EnterBossRaidMap;
        MainUi.OnBossRaidEnd += ExitBossRaidMap;
    }

    private void OnDisable()
    {
        MainUi.OnBossRaidStart -= EnterBossRaidMap;
        MainUi.OnBossRaidEnd -= ExitBossRaidMap;
    }


    public async UniTask Init()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[MapManager]: SaveManager가 없습니다.");
            return;
        }
        if (NetworkManager.Instance == null || NetworkManager.Instance.StageService == null)
        {
            Debug.LogError("[MapManager]: StageService가 없습니다.");
            return;
        }

        int savedStage = SaveManager.Instance.GetCurrentStage();
        NetworkManager.Instance.StageService.SetStageOnLoad(savedStage);
        await StartNewStage(savedStage);
        Debug.Log("MapManager 호출");
    }

    public void EnterBossRaidMap()
    {
        if(ObjectManager.Instance != null)
        {
            PlayerPartyController mainParty = ObjectManager.Instance.GetCurrentPlayerParty();
            if (mainParty != null)
            {
                mainParty.SetCameraActive(false); 
            }
        }

        LoadMapSpriteForBoss(_mapAddressableKeyForBoss).Forget();

        if (NetworkManager.Instance == null || NetworkManager.Instance.BossRaidService == null)
        {
            Debug.LogError("[MapManager] BossRaidService가 없습니다.");
            return;
        }

        string bossMonsterId = NetworkManager.Instance.BossRaidService.GetSelectedBossMonsterId();

        if (string.IsNullOrEmpty(bossMonsterId))
        {
            Debug.LogError("[MapManager] 선택된 보스의 MonsterId가 없습니다.");
            return;
        }

        if (ObjectManager.Instance != null)
        {
            ObjectManager.Instance.SpawnBossRaidEntities(bossMonsterId).Forget();
        }
    }

    public void ExitBossRaidMap()
    {
        if (ObjectManager.Instance != null)
        {
            PlayerPartyController mainParty = ObjectManager.Instance.GetCurrentPlayerParty();
            if (mainParty != null)
            {
                mainParty.SetCameraActive(true);
            }
        }

        if (NetworkManager.Instance != null && NetworkManager.Instance.CharacterStatusService != null)
        {
            NetworkManager.Instance.CharacterStatusService.ClearBossParty();
        }
    }

    public async UniTask StartNewStage(int stage)
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.StageService == null)
        {
            Debug.LogError("[MapManager]: StageService가 없습니다.");
            return;
        }
        if(stage < 1)
        {
            stage = 1;
        }
        StageService stageService = NetworkManager.Instance.StageService;

        if (stageService.CurrentStage != stage)
        {
            stageService.SetStage(stage);
        }

        await ChangeMapBasedOnStage(CurrentStage);
        OnStageChanged?.Invoke(CurrentStage);
    }

    public void ClearedCurrentStage()
    {
        OnStageCleared?.Invoke();
    }

    public void FailedCurrentStage()
    {
        OnStageFailed?.Invoke();
    }

    private async UniTask ChangeMapBasedOnStage(int currentStage)
    {
        if (_mapAddressableKeys == null || _mapAddressableKeys.Length == 0)
        {
            Debug.LogError("[MapManager]: 맵 Addressable Key가 없습니다.");
            return;
        }

        int mapIndex = ((currentStage - 1) % (_mapAddressableKeys.Length * 10)) / 10;

        if (mapIndex >= _mapAddressableKeys.Length)
        {
            Debug.LogError("스테이지 단계에 맞는 맵이 존재 X");
        }

        await LoadMapSprite(_mapAddressableKeys[mapIndex]);
    }

    private async UniTask LoadMapSprite(string mapKey)
    {
        if (_mapBackGround == null)
        {
            Debug.LogError("[MapManager]: 맵 배경 SpriteRenderer가 연결되지 않았습니다.");
            return;
        }

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

    private async UniTask LoadMapSpriteForBoss(string mapKey)
    {
        if (_mapBackGroundForBoss == null)
        {
            Debug.LogError("[MapManager]: 보스 맵 배경 SpriteRenderer가 연결되지 않았습니다.");
            return;
        }

        try
        {
            Sprite loadedSprite = await Addressables.LoadAssetAsync<Sprite>(mapKey);

            if (loadedSprite != null)
            {
                _mapBackGroundForBoss.sprite = loadedSprite;
                Debug.Log("맵 이미지 불러오기 성공");
            }
        }

        catch
        {
            Debug.LogError("맵 이미지를 불러기 실패");
        }
    }

    public Transform GetPlayerSpawnSpot()
    {
        return _playerSpawnSpot;
    }

    public Transform GetPlayerSpawnSpotForBoss()
    {
        return _playerSpawnSpotForBoss;
    }

    public Transform GetPlayerClearSpot()
    {
        return _playerClearSpot;
    }

    public Transform[] GetMonsterSpawnSpot()
    {
        return new Transform[] { _monsterSpawnSpot1, _monsterSpawnSpot2, _monsterSpawnSpot3 };
    }

    public Transform GetMonsterSpawnSpotForBoss()
    {
        return _monsterSpawnSpotForBoss;
    }
}