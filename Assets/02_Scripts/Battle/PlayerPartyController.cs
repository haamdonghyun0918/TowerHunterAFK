using System.Collections.Generic;
using UnityEngine;

public class PlayerPartyController : PlayerPartyControllerBase
{
    private float moveSpeed = 10.0f;
    public bool _isBattling = false;
    public bool _isMovable = false;
    public bool _isPaused = false;

    private void OnEnable()
    {
        MainUi.OnBossRaidStart += PauseMovement;
        MainUi.OnBossRaidEnd += ResumeMovement;
    }

    private void OnDisable()
    {
        MainUi.OnBossRaidStart -= PauseMovement;
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
            for (int i = 0; i < 3; i++) {
                var character = GetHunter(i);
                character.RunningCharacter();
            }
        }
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
            MapManager.Instance.ClearedCurrentStage();
        }
    }
}
