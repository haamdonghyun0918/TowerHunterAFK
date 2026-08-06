using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

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
    public void StartBattle(PlayerParty playerParty, GameObject monsterParty)
    {
        Debug.Log("전투 시작!");

        //TODO: 전투 로직 구현

        EndBattle(playerParty, monsterParty);
    }

    public void EndBattle(PlayerParty playerParty, GameObject monsterParty)
    {
        monsterParty.SetActive(false);
        Debug.Log("전투 종료!");

        playerParty.isBattling = false;
    }
}