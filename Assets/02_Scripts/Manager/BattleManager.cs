using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public PlayerPartyController _playerParty;

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
    
    public void StartBattle(PlayerPartyController playerParty, GameObject monsterParty)
    {
        Debug.Log("전투 시작!");
        MonsterParty enemyParty = monsterParty.GetComponent<MonsterParty>();

        AutoBattleRoutine(playerParty, enemyParty).Forget();
    }

    private async UniTaskVoid AutoBattleRoutine(PlayerPartyController playerParty, MonsterParty enemyParty)
    {
        List<BattleCharacter> turnQueue = new List<BattleCharacter>();

        for(int i = 0; i < 3; i++)
        {
            Character hunter = playerParty.GetHunter(i);
            if((hunter != null) && (hunter._isDead == false))
            {
                turnQueue.Add(hunter);
            }

            Monster monster = enemyParty.GetMonster(i);
            if((monster != null) && (monster._isDead == false))
            {
                turnQueue.Add(monster);
            }
        }

        turnQueue.Sort(CompareActionOrder);

        while ((playerParty.GetCurrentHunterCount() > 0) && (enemyParty.GetCurrentMonsterCount() > 0))
        {
            foreach (BattleCharacter curUnit in turnQueue)
            {
                if (curUnit._isDead == true)
                {
                    continue;
                }

                Transform attackerTransform = curUnit.transform;
                Transform targetTransform = null;
                bool isSkill = false;

                if (curUnit is Character hunter)
                {
                    Monster target = FindMonsterTarget(enemyParty);
                    if (target == null)
                    {
                        continue;
                    }

                    targetTransform = target.transform;

                    Vector3 originPos = attackerTransform.position;
                    attackerTransform.position = (originPos + targetTransform.position) / 2f;
                    await UniTask.Delay(isSkill ? 1000 : 500);

                    if (hunter._isDead == true)
                    {
                        continue;
                    }

                    hunter.AtkTarget(target);
                    await UniTask.Delay(500);

                    if (hunter._isDead == false)
                    {
                        attackerTransform.position = originPos;
                    }
                }
                else if (curUnit is Monster monster)
                {
                    Character target = FindHunterTarget(playerParty);
                    if (target == null)
                    {
                        continue;
                    }

                    targetTransform = target.transform;
                    Vector3 originPos = attackerTransform.position;
                    attackerTransform.position = (originPos + targetTransform.position) / 2f;
                    await UniTask.Delay(500);

                    if (monster._isDead == true)
                    {
                        continue;
                    }

                    monster.AtkTarget(target);
                    await UniTask.Delay(500);

                    if (monster._isDead == false)
                    {
                        attackerTransform.position = originPos;
                    }
                }

                bool isHunterOrMonsterWipeOut = (playerParty.GetCurrentHunterCount() == 0) || (enemyParty.GetCurrentMonsterCount() == 0);
                
                if (isHunterOrMonsterWipeOut)
                {
                    break;
                }
            }
        }

        if (playerParty.GetCurrentHunterCount() == 0)
        {
            Debug.Log("헌터 파티가 모두 리타이어 했습니다. 안전지대로 돌아갑니다.");
            MapManager.Instance.FailedCurrentStage();
        }
        else if (enemyParty.GetCurrentMonsterCount() == 0)
        {
            Debug.Log("헌터 파티 승리!");
        }

        EndBattle(playerParty, enemyParty.gameObject);
    }

    private int CompareActionOrder(BattleCharacter a, BattleCharacter b)
    {
        if (a._characterAtkSpeed != b._characterAtkSpeed)
        {
            return b._characterAtkSpeed.CompareTo(a._characterAtkSpeed);
        }

        bool aIsPlayer = a is Character;
        bool bIsPlayer = b is Character;

        if ((aIsPlayer == true) && (bIsPlayer == false))
        {
            return -1;
        }

        if ((aIsPlayer == false) && (bIsPlayer == true))
        {
            return 1;
        }

        return 0;
    }

    private Monster FindMonsterTarget(MonsterParty enemyParty)
    {
        for (int i = 0; i < 3;  i++)
        {
            Monster target = enemyParty.GetMonster(i);
            if ((target != null) && (target._isDead == false))
            {
                return target;
            }
        }

        return null;
    }

    private Character FindHunterTarget(PlayerPartyController playerParty)
    {
        for (int i = 0; i < 3; i++)
        {
            Character target = playerParty.GetHunter(i);
            if ((target != null) && (target._isDead == false))
            {
                return target;
            }
        }

        return null;
    }

    public void EndBattle(PlayerPartyController playerParty, GameObject monsterParty)
    {
        Debug.Log("전투 종료!");
        monsterParty.SetActive(false);
        playerParty._isBattling = false;
        playerParty._isMovable = true;
    }
}