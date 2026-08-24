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
        //TODO: 보스레이드 끝났을 때 이벤트 구독
    }

    private void OnDisable()
    {
        MainUi.OnBossRaidStart -= PauseMovement;
        //TODO: 보스레이드 끝났을 때 이벤트 해제
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
