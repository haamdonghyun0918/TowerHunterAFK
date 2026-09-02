using UnityEngine;

public class PlayerPartyController : PlayerPartyControllerBase
{
    private float moveSpeed = 10.0f;
    public bool _isBattling = false;
    public bool _isMovable = false;
    public bool _isPaused = false;

    private void OnEnable()
    {
        MainUi.OnBossRaidUiOpen += PauseMovement;
        MainUi.OnBossRaidEnd += ResumeMovement;
    }

    private void OnDisable()
    {
        MainUi.OnBossRaidUiOpen -= PauseMovement;
        MainUi.OnBossRaidEnd -= ResumeMovement;
    }

    public void PauseMovement()
    {
        _isPaused = true;
    }

    public void ResumeMovement()
    {
        _isPaused = false;
    }

    private void Update()
    {
        if ((_isPaused == false) && (_isBattling == false) && (_isMovable == true))
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            for (int i = 0; i < MaxPartySize; i++) 
            {
                var character = GetHunter(i);
                if (character != null)
                {
                    character.RunningCharacter();
                }
            }
        }
    }

    public void SyncPartyHpToSaveData()
    {
        if (SaveManager.Instance == null) return;

        foreach (var hunter in _hunters)
        {
            if (hunter != null)
            {
                string uid = hunter.GetCharacterUniqueId();
                if (SaveManager.Instance.CharacterDict.TryGetValue(uid, out var saveData))
                {
                    saveData.CurrentHP = hunter._isDead ? 0 : hunter.GetCurrentHp();
                }
            }
        }

        SaveManager.Instance.SaveCurrentData(); 
        Debug.Log("[PlayerPartyController] 다음 층 진입: 현재 파티의 체력을 저장했습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MonsterParty"))
        {
            _isBattling = true;
            _isMovable = false;

            GameObject encounteredMonsterParty = other.gameObject;
            BattleManager.Instance.StartBattle(this, encounteredMonsterParty);
        }
        else if (other.CompareTag("ClearSpot"))
        {
            _isMovable = false;
            SyncPartyHpToSaveData();
            MapManager.Instance.ClearedCurrentStage();
        }
    }
}
