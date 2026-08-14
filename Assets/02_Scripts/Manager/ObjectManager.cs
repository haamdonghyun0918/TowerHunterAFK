using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_PlayerParty;
    [SerializeField] private GameObject Prefab_MonsterParty;

    [SerializeField] private GameObject Prefab_TestDefaultPlayer;
    [SerializeField] private GameObject Prefab_TestDefaultMonster;

    private PlayerPartyController _currentPlayerParty;
    private List<MonsterParty> _currentMonsterParty = new List<MonsterParty>();

    public static ObjectManager Instance { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public async UniTask SpawnEntities(int stage)
    {
        ClearCurrentEntities();

        Transform playerSpawnSpot = MapManager.Instance.GetPlayerSpawnSpot();
        Transform[] monsterSpawnSpots = MapManager.Instance.GetMonsterSpawnSpot();

        if (playerSpawnSpot != null)
        {
            if (_currentPlayerParty == null)
            {
                if (Prefab_PlayerParty != null)
                {
                    GameObject gObj_PlayerParty = Instantiate(Prefab_PlayerParty, playerSpawnSpot.position, Quaternion.identity);
                    _currentPlayerParty = gObj_PlayerParty.GetComponent<PlayerPartyController>();

                    //[TODO] : 나중에는 덱 편성/세이브데이터를 받아와서 세팅해줄것.
                    //테스트용 하드코딩
                    string[] testHunterIds = { "character_Test_01" };

                    foreach (string hunterId in testHunterIds)
                    {
                        await SpawnHunter(hunterId);
                    }
                }
            }
            else
            {
                _currentPlayerParty.transform.position = playerSpawnSpot.position;
                _currentPlayerParty._isBattling = false;
            }

            if (_currentPlayerParty.GetCurrentHunterCount() == 0)
            {
                _currentPlayerParty.MakeFullHPAllHunters();
            }

            //추가
            if(_currentPlayerParty != null)
            {
                if(NetworkManager.Instance == null || NetworkManager.Instance.CharacterStatusService  == null)
                {
                    Debug.LogError("[ObjectManager] CharacterStatusService가 없습니다.");
                }
                else
                {
                    NetworkManager.Instance.CharacterStatusService.SetParty(_currentPlayerParty);
                }
            }
            //끝
        }

        int maxCleared = 0;
        if(NetworkManager.Instance != null && NetworkManager.Instance.StageService != null)
        {
            maxCleared = NetworkManager.Instance.StageService.GetStageViewModel().MaxClearedStage;
        }
        bool isRestArea = (stage % 10 == 0) && (maxCleared >= stage);

        if (isRestArea)
        {
            if (_currentPlayerParty != null)
            {
                _currentPlayerParty.MakeFullHPAllHunters();
            }
        }
        else
        {
            //[TODO] : 나중에는 Stage 숫자를 기반으로 맵 데이터에서 등장 몬스터 Id를 가져오거나 할 것.
            if ((Prefab_MonsterParty != null) && (monsterSpawnSpots != null))
            {
                foreach (Transform spot in monsterSpawnSpots)
                {
                    if (spot == null)
                    {
                        continue;
                    }

                    GameObject gObj_MonsterParty = Instantiate(Prefab_MonsterParty, spot.position, Quaternion.identity);
                    MonsterParty newMonsterParty = gObj_MonsterParty.GetComponent<MonsterParty>();

                    string[] testMonsterIds = { "monster_Test_01", "monster_Test_01" };
                    foreach (string monsterId in testMonsterIds)
                    {
                       await SpawnMonster(monsterId, newMonsterParty);
                    }

                    _currentMonsterParty.Add(newMonsterParty);
                }
            }
        }

        if (_currentPlayerParty != null)
        {
            _currentPlayerParty._isMovable = true;

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance._playerParty = _currentPlayerParty;
            }
        }
    }

    private async UniTask SpawnHunter(string characterId)
    {
        var data = GameDataManager.Instance.GetData<CharacterData>(characterId);
        if (data == null)
        {
            Debug.LogError($"[ObjectManager] characterId {characterId} 데이터를 찾을 수 없습니다!");
            return;
        }

        GameObject hunterPrefab = Prefab_TestDefaultPlayer;
        if (string.IsNullOrEmpty(data.PrefabPath) == false)
        {
            GameObject loadedPrefab = await Addressables.LoadAssetAsync<GameObject>(data.PrefabPath);
            if (loadedPrefab != null)
            {
                hunterPrefab = loadedPrefab;
            }
            else
            {
                Debug.LogWarning($"[ObjectManager] {data.PrefabPath} 경로에서 프리팹을 찾을 수 없습니다. 기본 프리팹을 사용합니다.");
            }
        }

        if (hunterPrefab != null)
        {
            GameObject hunterObj = Instantiate(hunterPrefab);
            Character newHunter = hunterObj.GetComponent<Character>();

            _currentPlayerParty.AddHunter(newHunter);
        }
    }

    private async UniTask SpawnMonster(string monsterId, MonsterParty targetMonsterParty)
    {
        var data = GameDataManager.Instance.GetData<MonsterData>(monsterId);
        if (data == null)
        {
            Debug.LogError($"[ObjectManager] monsterId {monsterId} 데이터를 찾을 수 없습니다!");
            return;
        }

        GameObject prefabToSpawn = Prefab_TestDefaultMonster;

        if (string.IsNullOrEmpty(data.PrefabPath) == false)
        {
            GameObject loadedPrefab = await Addressables.LoadAssetAsync<GameObject>(data.PrefabPath);
            if (loadedPrefab != null)
            {
                prefabToSpawn = loadedPrefab;
            }
        }

        if (prefabToSpawn != null)
        {
            GameObject mobObj = Instantiate(prefabToSpawn);
            Monster newMonster = mobObj.GetComponent<Monster>();

            targetMonsterParty.AddMonster(newMonster);
        }
        else
        {
            Debug.LogError($"[ObjectManager] SpawnMonster: {monsterId}를 생성할 프리팹이 할당되지 않았습니다!");
        }
    }

    private void ClearCurrentEntities()
    {
        foreach (var monsterParty in _currentMonsterParty)
        {
            if (monsterParty != null)
            {
                //ToDo 경인: 수정
                Destroy(monsterParty.gameObject);
            }
        }
        
        _currentMonsterParty.Clear();
    }
}
