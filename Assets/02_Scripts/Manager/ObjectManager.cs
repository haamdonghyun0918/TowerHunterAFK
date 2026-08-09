using Cysharp.Threading.Tasks.Triggers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_PlayerParty;
    [SerializeField] private GameObject Prefab_MonsterParty;

    [SerializeField] private GameObject Prefab_TestDefaultPlayerParty;
    [SerializeField] private GameObject Prefab_TestDefaultMonsterParty;

    private PlayerPartyController _currentPlayerParty;
    private MonsterParty _currentMonsterParty;

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

    public void SpawnEntities(int stage)
    {
        ClearCurrentEntities();

        Transform playerSpawnSpot = MapManager.Instance.GetPlayerSpawnSpot();
        Transform[] monsterSpawnSpots = MapManager.Instance.GetMonsterSpawnSpot();

        if ((Prefab_PlayerParty != null) && (playerSpawnSpot != null))
        {
            GameObject gObj_PlayerParty = Instantiate(Prefab_PlayerParty, playerSpawnSpot.position, Quaternion.identity);
            _currentPlayerParty = gObj_PlayerParty.GetComponent<PlayerPartyController>();

            //[TODO] : 나중에는 덱 편성/세이브데이터를 받아와서 세팅해줄것.
            string[] testHunterIds = { "character_Test_01" };

            foreach (string hunterId in testHunterIds)
            {
                SpawnHunter(hunterId);
            }
        }

        if ((Prefab_MonsterParty != null) && (monsterSpawnSpots != null))
        {
            GameObject gObj_MonsterParty = Instantiate(Prefab_MonsterParty, monsterSpawnSpots[0].position, Quaternion.identity);
            _currentMonsterParty = gObj_MonsterParty.GetComponent<MonsterParty>();

            //[TODO] : 나중에는 Stage 숫자를 기반으로 맵 데이터에서 등장 몬스터 Id를 가져오거나 할 것.
            string[] testMonsterIds = { "monster_Test_01", "monster_Test_01" };

            foreach (string monsterId in testMonsterIds)
            {
                SpawnMonster(monsterId);
            }
        }

        if (_currentPlayerParty != null)
        {
            _currentPlayerParty._isMovable = true;
        }
    }

    private void SpawnHunter(string characterId)
    {
        var data = GameDataManager.Instance.GetData<CharacterData>(characterId);
        if (data == null)
        {
            Debug.LogError($"[ObjectManager] characterId {characterId} 데이터를 찾을 수 없습니다!");
            return;
        }

        GameObject hunterPrefab = Prefab_TestDefaultPlayerParty;
        if (string.IsNullOrEmpty(data.PrefabPath) == false)
        {
            GameObject loadedPrefab = Resources.Load<GameObject>(data.PrefabPath);
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

            //기존의 Character, Monster의 Start부분에서 있었던 데이터 초기화를 Init(string id)로 뺴주시면 됩니다!
            //newHunter.Init(characterId); 
            _currentPlayerParty.AddHunter(newHunter);
        }
    }

    private void SpawnMonster(string monsterId)
    {
        var data = GameDataManager.Instance.GetData<MonsterData>(monsterId);
        if (data == null)
        {
            Debug.LogError($"[ObjectManager] monsterId {monsterId} 데이터를 찾을 수 없습니다!");
            return;
        }

        GameObject prefabToSpawn = Prefab_TestDefaultMonsterParty;

        //Monster에는 프리팹 경로가 없네용
        //if (!string.IsNullOrEmpty(data.PrefabPath))
        //{
        //    GameObject loadedPrefab = Resources.Load<GameObject>(data.PrefabPath);
        //    if (loadedPrefab != null) prefabToSpawn = loadedPrefab;
        //}

        if (prefabToSpawn != null)
        {
            GameObject mobObj = Instantiate(prefabToSpawn);
            Monster newMonster = mobObj.GetComponent<Monster>();
            
            //상동.
            //newMonster.Init(monsterId);

            _currentMonsterParty.AddMonster(newMonster);
        }
    }

    private void ClearCurrentEntities()
    {
        if (_currentPlayerParty != null)
        {
            Destroy(_currentPlayerParty.gameObject);
            _currentPlayerParty = null;
        }

        if (_currentMonsterParty != null)
        {
            Destroy(_currentMonsterParty.gameObject);
            _currentMonsterParty = null;
        }
    }



}
