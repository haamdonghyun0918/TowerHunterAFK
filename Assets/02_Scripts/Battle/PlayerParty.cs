using UnityEngine;

public class PlayerParty : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public bool isBattling = false;

    private void Update()
    {
        if (isBattling == false)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MonsterParty"))
        {
            isBattling = true;

            GameObject encounteredMonsterParty = other.gameObject;
            BattleManager.Instance.StartBattle(this, encounteredMonsterParty);
        }
    }
}
