using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System;

public class MapManager : MonoBehaviour
{
    [Header("Player Spawn & Clear Spot")]
    [SerializeField] private Transform _playerSpawnSpot;
    [SerializeField] private Transform _playerClearSpot;

    [Header("MonsterSpawnSpot")]
    [SerializeField] private GameObject _monsterSpawnSpot1;
    [SerializeField] private GameObject _monsterSpawnSpot2;
    [SerializeField] private GameObject _monsterSpawnSpot3;

    [Header("Map Addressables")]
    [SerializeField] private SpriteRenderer _mapBackGround;
    [SerializeField] private string[] _mapAddressableKeys = { "Map1", "Map2", "Map3" };

    private int _currentStage;
    public event Action<int> OnStageChanged;
    public event Action OnStageCleared;

    private void Start()
    {
        StartNewStage(1);
        // TODO: SaveManager 생성 시 변경해야 할 최우선 로직
    }

    private void StartNewStage(int stage)
    {
        _currentStage = stage;
        ChangeMapBasedOnStage(_currentStage).Forget();
        OnStageChanged?.Invoke(_currentStage);
    }

    private void ClearedCurrentStage()
    {
        OnStageCleared?.Invoke();
        StartNewStage(_currentStage + 1);
    }

    private async UniTaskVoid ChangeMapBasedOnStage(int currentStage)
    {
        // 1 ~ 10: 맵 어드레서블 키 0번, 11~ 20: 맵 어드레서블 키 1번 이런식으로 진행
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