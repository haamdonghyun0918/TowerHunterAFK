using UnityEngine;

public class PlayerPartyControllerForBoss : PlayerPartyControllerBase
{
    private float moveSpeed = 10.0f;
    public bool _isBattling = false;
    public bool _isMovable = false;

    private void Update()
    {
        if ((_isBattling == false) && (_isMovable == true))
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
    }
}
