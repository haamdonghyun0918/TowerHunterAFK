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
                int attackerIndex = -1;

                if (curUnit is Character hunter)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (playerParty.GetHunter(i) == hunter)
                        {
                            attackerIndex = i;
                            break;
                        }
                    }

                    Monster target = FindMonsterTarget(enemyParty, attackerIndex);
                    if (target == null)
                    {
                        continue;
                    }

                    targetTransform = target.transform;
                    Vector3 originPos = attackerTransform.position;
                    attackerTransform.position = (originPos + targetTransform.position) / 2f;

                    if (hunter._isDead == true)
                    {
                        continue;
                    }

                    await UniTask.Delay(300);
                    await hunter.AtkTarget(target);

                    if (hunter._isDead == false)
                    {
                        await UniTask.Delay(300);
                        attackerTransform.position = originPos;
                        await UniTask.Delay(500);
                    }
                }
                else if (curUnit is Monster monster)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (enemyParty.GetMonster(i) == monster)
                        {
                            attackerIndex = i;
                            break;
                        }
                    }

                    Character target = FindHunterTarget(playerParty, attackerIndex);
                    if (target == null)
                    {
                        continue;
                    }

                    targetTransform = target.transform;
                    Vector3 originPos = attackerTransform.position;
                    attackerTransform.position = (originPos + targetTransform.position) / 2f;
                    await UniTask.Delay(300);

                    if (monster._isDead == true)
                    {
                        continue;
                    }

                    monster.AtkTarget(target);
                    await UniTask.Delay(300);

                    if (monster._isDead == false)
                    {
                        attackerTransform.position = originPos;
                        await UniTask.Delay(300);
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

    private Monster FindMonsterTarget(MonsterParty enemyParty, int attackerIndex)
    {
        if (attackerIndex >= 0 && attackerIndex < 3)
        {
            Monster frontTarget = enemyParty.GetMonster(attackerIndex);
            if (frontTarget != null && frontTarget._isDead == false)
            {
                return frontTarget; 
            }
        }

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

    private Character FindHunterTarget(PlayerPartyController playerParty, int attackerIndex)
    {
        if (attackerIndex >= 0 && attackerIndex < 3)
        {
            Character frontTarget = playerParty.GetHunter(attackerIndex);
            if (frontTarget != null && frontTarget._isDead == false)
            {
                return frontTarget;
            }
        }

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