using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_PlayerParty;
    [SerializeField] private GameObject Prefab_MonsterParty;
    [SerializeField] private GameObject Prefab_BossPlayerParty;

    private PlayerPartyController _currentPlayerParty;
    private PlayerPartyCamera _currentPlayerPartyCamera;

    private List<MonsterParty> _monsterPartyList = new List<MonsterParty>();

    private Queue<MonsterParty> _monsterPartyPool = new Queue<MonsterParty>();
    private Dictionary<string, Queue<Monster>> _monsterPoolDictionary = new Dictionary<string, Queue<Monster>>();

    private List<string> _normalMonsterIdList = new List<string>();

    private int _curStageNum;

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
        StageService stageService = NetworkManager.Instance.StageService;
        _curStageNum = stageService.CurrentStage;

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
                    partySetting.CreateHunterParty();

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
                            bool isSpawned = await SpawnHunter(targetData.BaseId, targetData.UniqueId, _currentPlayerParty);

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
                
                for (int i = 0; i < _currentPlayerParty.MaxPartySize; i++)
                {
                    Character hunter = _currentPlayerParty.GetHunter(i);
                    if (hunter != null && hunter._isDead == false)
                    {
                        hunter.RefreshStatFromSaveData();
                    }
                }
            }

            if (_currentPlayerParty != null)
            {
                SetCurrentPlayerPartyCamera(_currentPlayerParty.gameObject);
            }

            if (_currentPlayerParty.GetCurrentHunterCount() == 0)
            {
                _currentPlayerParty.MakeFullHPAllHunters();

                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveCurrentData();
                }
            }

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
        }

        int maxCleared = 0;
        if (NetworkManager.Instance != null && NetworkManager.Instance.StageService != null)
        {
            maxCleared = NetworkManager.Instance.StageService.MaxClearedStage;
        }

        bool isRestArea = ((stage % 10 == 0) && (maxCleared >= stage));
        bool isFirstFloor = (stage == 1);

        if (isRestArea)
        {
            if (_currentPlayerParty != null)
            {
                _currentPlayerParty.MakeFullHPAllHunters();

                if (SaveManager.Instance != null)
                {
                    foreach(var characterSave in SaveManager.Instance.CharacterDict.Values)
                    {
                        characterSave.CurrentHP = -1;
                    }

                    SaveManager.Instance.SaveCurrentData();
                }
            }
        }
        else if (isFirstFloor)
        {
            if (_currentPlayerParty != null)
            {
                _currentPlayerParty.MakeFullHPAllHunters();

                if (SaveManager.Instance != null)
                {
                    foreach (var characterSave in SaveManager.Instance.CharacterDict.Values)
                    {
                        characterSave.CurrentHP = -1;
                    }

                    SaveManager.Instance.SaveCurrentData();
                }
            }

            InitNormalMonsterList();

            if ((Prefab_MonsterParty != null) && (monsterSpawnSpots != null))
            {
                foreach (Transform spot in monsterSpawnSpots)
                {
                    if (spot == null)
                    {
                        continue;
                    }

                    MonsterParty newMonsterParty = GetOrCreateMonsterParty(spot.position);

                    if (_normalMonsterIdList.Count > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, _normalMonsterIdList.Count);
                            string randomMonsterId = _normalMonsterIdList[randomIndex];
                            await SpawnMonster(randomMonsterId, newMonsterParty);
                        }
                    }

                    _monsterPartyList.Add(newMonsterParty);
                }
            }
        }
        else
        {
            InitNormalMonsterList();

            if ((Prefab_MonsterParty != null) && (monsterSpawnSpots != null))
            {
                foreach (Transform spot in monsterSpawnSpots)
                {
                    if (spot == null)
                    {
                        continue;
                    }

                    MonsterParty newMonsterParty = GetOrCreateMonsterParty(spot.position);

                    if (_normalMonsterIdList.Count > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, _normalMonsterIdList.Count);
                            string randomMonsterId = _normalMonsterIdList[randomIndex];
                            await SpawnMonster(randomMonsterId, newMonsterParty);
                        }
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

    public async UniTaskVoid SpawnBossRaidEntities(string bossMonsterId)
    {
        StageService stageService = NetworkManager.Instance.StageService;
        _curStageNum = stageService.CurrentStage;

        Transform playerSpawnSpotForBoss = MapManager.Instance.GetPlayerSpawnSpotForBoss();
        Transform monsterSpawnSpotForBoss = MapManager.Instance.GetMonsterSpawnSpotForBoss();

        if (Prefab_BossPlayerParty == null || Prefab_MonsterParty == null)
        {
            Debug.LogError("[ObjectManager] 보스 레이드 프리팹이 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(bossMonsterId))
        {
            Debug.LogError("[ObjectManager] 생성할 보스 MonsterId가 없습니다.");
            return;
        }

        GameObject gObj_BossParty = Instantiate(Prefab_BossPlayerParty, playerSpawnSpotForBoss.position, Quaternion.identity);
        PlayerPartyControllerForBoss bossParty = gObj_BossParty.GetComponent<PlayerPartyControllerForBoss>();

        if (bossParty == null)
        {
            Debug.LogError("[ObjectManager] PlayerPartyControllerForBoss가 없습니다.");
            Destroy(gObj_BossParty);
            return;
        }

        SetCurrentPlayerPartyCamera(gObj_BossParty);

        string[] bossPartyUids = SaveManager.Instance.CurrentSaveData.BossRaidPartyUids;

        for (int i = 0; i < bossPartyUids.Length; i++)
        {
            string uid = bossPartyUids[i];

            if (string.IsNullOrEmpty(uid) == false && SaveManager.Instance.CharacterDict.TryGetValue(uid, out CharacterSaveData charData))
            {
                await SpawnHunter(charData.BaseId, uid, bossParty);
            }
        }

        if (NetworkManager.Instance == null || NetworkManager.Instance.CharacterStatusService == null)
        {
            Debug.LogError("[ObjectManager] CharacterStatusService가 없습니다.");
        }
        else
        {
            NetworkManager.Instance.CharacterStatusService.SetBossParty(bossParty);
        }

        GameObject gObj_BossMonsterParty = Instantiate(Prefab_MonsterParty, monsterSpawnSpotForBoss.position, Quaternion.identity);
        MonsterParty bossMonsterParty = gObj_BossMonsterParty.GetComponent<MonsterParty>();

        await SpawnMonster(bossMonsterId, bossMonsterParty);

        BossRaidBattleUI bossRaidBattleUI = await UiManager.Instance.OpenUi<BossRaidBattleUI>(); 

        if (bossRaidBattleUI != null)
        {
            var bossData = GameDataManager.Instance.GetData<MonsterData>(bossMonsterId);
            string bossName = bossData != null ? bossData.Name : "보스 몬스터";

            bossRaidBattleUI.Init(bossParty, bossMonsterParty, bossName);
            bossParty._isMovable = true;
        }

        UiManager.Instance.CloseUi<MainUi>();   
    }

    private async UniTask<bool> SpawnHunter(string characterId, string characterUniqueId, PlayerPartyControllerBase targetParty)
    {
        var data = GameDataManager.Instance.GetData<CharacterData>(characterId);
        if (data == null)
        {
            Debug.LogError($"[ObjectManager] characterId {characterId} 데이터를 찾을 수 없습니다!");
            return false;
        }

        GameObject hunterPrefab = null;
        if (string.IsNullOrEmpty(data.PrefabPath) == false)
        {
            GameObject loadedPrefab = await Addressables.LoadAssetAsync<GameObject>(data.PrefabPath);
            if (loadedPrefab != null)
            {
                hunterPrefab = loadedPrefab;
            }
            else
            {
                Debug.LogWarning($"[ObjectManager] hunterPrefab이 null입니다!.");
                return false;
            }
        }

        if (hunterPrefab != null)
        {
            GameObject hunterObj = Instantiate(hunterPrefab);
            Character newHunter = hunterObj.GetComponent<Character>();

            bool isBossRaid = targetParty is PlayerPartyControllerForBoss;
            newHunter.InitCharacter(data, characterUniqueId, isBossRaid);
            targetParty.AddHunter(newHunter);
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
            reuseMonster.InitMonster(data, _curStageNum);
            targetMonsterParty.AddMonster(reuseMonster);
            return;
        }

        GameObject prefabToSpawn = null;

        if (string.IsNullOrEmpty(data.PrefabPath) == false)
        {
            GameObject loadedPrefab = await Addressables.LoadAssetAsync<GameObject>(data.PrefabPath);
            if (loadedPrefab != null)
            {
                prefabToSpawn = loadedPrefab;
            }
            else
            {
                Debug.LogWarning("몬스터 프리팹이 널입니다.");
                return;
            }
        }

        if (prefabToSpawn != null)
        {
            GameObject mobObj = Instantiate(prefabToSpawn);
            Monster newMonster = mobObj.GetComponent<Monster>();
            newMonster.InitMonster(data, _curStageNum);
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

    public void ClearPlayerParty()
    {
        if (_currentPlayerParty != null)
        {
            Destroy(_currentPlayerParty.gameObject);
            _currentPlayerParty = null;
            _currentPlayerPartyCamera = null;
        }
    }

    public PlayerPartyController GetCurrentPlayerParty()
    {
        return _currentPlayerParty;
    }

    public PlayerPartyCamera GetCurrentPlayerPartyCamera()
    {
        return _currentPlayerPartyCamera;
    }

    private void SetCurrentPlayerPartyCamera(GameObject partyObject)
    {
        if (partyObject == null)
        {
            _currentPlayerPartyCamera = null;
            return;
        }

        _currentPlayerPartyCamera = partyObject.GetComponentInChildren<PlayerPartyCamera>(true);

        if (_currentPlayerPartyCamera == null)
        {
            Debug.LogWarning($"[ObjectManager] {partyObject.name} 안에 PlayerPartyCamera가 없습니다.");
        }
    }

    private void InitNormalMonsterList()
    {
        if (_normalMonsterIdList.Count > 0)
        {
            return;
        }

        List<MonsterData> allMonsters = GameDataManager.Instance.GetAllData<MonsterData>();

        if (allMonsters == null)
        {
            return;
        }

        foreach (var monster in allMonsters)
        {
            if (monster.IsBoss == false)
            {
                _normalMonsterIdList.Add(monster.Id);
            }
        }
    }
    
}
