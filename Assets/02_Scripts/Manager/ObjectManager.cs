using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_PlayerParty;
    [SerializeField] private GameObject Prefab_MonsterParty;

    [SerializeField] private GameObject Prefab_TestDefaultPlayer;
    [SerializeField] private GameObject Prefab_TestDefaultMonster;

    private PlayerPartyController _currentPlayerParty;
    private List<MonsterParty> _monsterPartyList = new List<MonsterParty>();

    private Queue<MonsterParty> _monsterPartyPool = new Queue<MonsterParty>();
    private Dictionary<string, Queue<Monster>> _monsterPoolDictionary = new Dictionary<string, Queue<Monster>>();

    public static ObjectManager Instance { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
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

                    PartySetting partySetting = new PartySetting();
                    partySetting.TestParty(); // 테스트용

                    string[] currentPartyUids = partySetting.GetCurrentPartyUids();

                    for (int i = 0; i < currentPartyUids.Length; i++)
                    {
                        string partyUid = currentPartyUids[i];

                        if (string.IsNullOrEmpty(partyUid))
                        {
                            continue;
                        }

                        if (SaveManager.Instance.CharacterDict.TryGetValue(partyUid, out CharacterSaveData targetData))
                        {
                            bool isSpawned = await SpawnHunter(targetData.BaseId);

                            if (isSpawned == true)
                            {
                                Debug.Log($"[오브젝트 매니저] {targetData.BaseId} 스폰 완료!");
                            }
                            else
                            {
                                Debug.LogWarning($"[오브젝트 매니저] {targetData.BaseId} 스폰 중단됨");

                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[오브젝트 매니저] 딕셔너리에서 ID: '{partyUid}'를 찾을 수 없습니다!");
                        }
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
            if (_currentPlayerParty != null)
            {
                if (NetworkManager.Instance == null || NetworkManager.Instance.CharacterStatusService == null)
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
        if (NetworkManager.Instance != null && NetworkManager.Instance.StageService != null)
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

                    MonsterParty newMonsterParty = GetOrCreateMonsterParty(spot.position);

                    string[] testMonsterIds = { "monster_Test_01", "monster_Test_02", "monster_Test_03" };
                    foreach (string monsterId in testMonsterIds)
                    {
                        await SpawnMonster(monsterId, newMonsterParty);
                    }

                    _monsterPartyList.Add(newMonsterParty);
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

    private async UniTask<bool> SpawnHunter(string characterId)
    {
        var data = GameDataManager.Instance.GetData<CharacterData>(characterId);
        if (data == null)
        {
            Debug.LogError($"[ObjectManager] characterId {characterId} 데이터를 찾을 수 없습니다!");
            return false;
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
            newHunter.InitCharacter(data);
            _currentPlayerParty.AddHunter(newHunter);
            return true;
        }

        return false;
    }

    private async UniTask SpawnMonster(string monsterId, MonsterParty targetMonsterParty)
    {
        var data = GameDataManager.Instance.GetData<MonsterData>(monsterId);
        if (data == null)
        {
            Debug.LogError($"[ObjectManager] monsterId {monsterId} 데이터를 찾을 수 없습니다!");
            return;
        }

        if ((_monsterPoolDictionary.TryGetValue(monsterId, out Queue<Monster> pool)) && (pool.Count > 0) == true)
        {
            Monster reuseMonster = pool.Dequeue();
            reuseMonster.gameObject.SetActive(true);
            reuseMonster.InitMonster(data);
            targetMonsterParty.AddMonster(reuseMonster);
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
            newMonster.InitMonster(data);
            newMonster._instanceId = monsterId;
            targetMonsterParty.AddMonster(newMonster);
        }
        else
        {
            Debug.LogError($"[ObjectManager] SpawnMonster: {monsterId}를 생성할 프리팹이 할당되지 않았습니다!");
        }
    }

    private MonsterParty GetOrCreateMonsterParty(Vector3 position)
    {
        if (_monsterPartyPool.Count > 0)
        {
            MonsterParty monsterParty = _monsterPartyPool.Dequeue();
            monsterParty.transform.position = position;
            monsterParty.gameObject.SetActive(true);
            return monsterParty;
        }
        else
        {
            GameObject gObj_monsterParty = Instantiate(Prefab_MonsterParty, position, Quaternion.identity);
            return gObj_monsterParty.GetComponent<MonsterParty>();
        }

    }

    private void ClearCurrentEntities()
    {
        foreach (var monsterParty in _monsterPartyList)
        {
            if (monsterParty != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Monster monster = monsterParty.GetMonster(i);
                    if (monster != null)
                    {
                        monster.gameObject.SetActive(false);
                        string monsterId = monster._instanceId;
                        if (string.IsNullOrEmpty(monsterId) == false) 
                        {
                            if (_monsterPoolDictionary.ContainsKey(monsterId) == false)
                            {
                                _monsterPoolDictionary[monsterId] = new Queue<Monster>();
                            }
                            _monsterPoolDictionary[monsterId].Enqueue(monster);
                        }
                    }
                }

                monsterParty.ClearParty();
                monsterParty.gameObject.SetActive(false);
                _monsterPartyPool.Enqueue(monsterParty);
            }
        }

        _monsterPartyList.Clear();
    }
}
