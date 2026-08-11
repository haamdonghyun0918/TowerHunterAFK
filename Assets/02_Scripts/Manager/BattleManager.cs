using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public PlayerPartyController _playerParty;

    public static BattleManager Instance { get; private set; }

    private class BattleEntity
    {
        public Character Hunter;
        public Monster Mob;
        public bool IsPlayer;
        public int Speed;
        public int Index;

        public bool IsDead
        {
            get
            {
                return IsPlayer ? Hunter._isDead : Mob._isDead;
            }
        }
    }

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
        List<BattleEntity> turnQueue = new List<BattleEntity>();

        for(int i = 0; i < 3; i++)
        {
            Character hunter = playerParty.GetHunter(i);
            if((hunter != null) && (hunter._isDead == false))
            {
                turnQueue.Add(new BattleEntity
                {
                    Hunter = hunter,
                    IsPlayer = true,
                    Speed = hunter._characterAtkSpeed,
                    Index = i
                });
            }

            Monster monster = enemyParty.GetMonster(i);
            if((monster != null) && (monster._isDead == false))
            {
                turnQueue.Add(new BattleEntity
                {
                    Mob = monster,
                    IsPlayer = false,
                    Speed = monster._characterAtkSpeed,
                    Index = i
                });
            }
        }

        turnQueue.Sort(CompareActionOrder);

        while ((playerParty.GetCurrentHunterCount() > 0) && (enemyParty.GetCurrentMonsterCount() > 0))
        {
            foreach (var entity in turnQueue)
            {
                if (entity.IsDead == true)
                {
                    continue;
                }

                Transform attackerTransform = null;
                Transform targetTransform = null;
                bool isSkill = false;

                if (entity.IsPlayer == true)
                {
                    Monster target = FindMonsterTarget(entity.Index, enemyParty);
                    if ((target == null) || (target._isDead == true))
                    {
                        continue;
                    }

                    attackerTransform = entity.Hunter.transform;
                    targetTransform = target.transform;
                }
                else
                {
                    Character target = FindHunterTarget(entity.Index, playerParty);
                    if ((target == null) || (target._isDead == true))
                    {
                        continue;
                    }

                    attackerTransform = entity.Mob.transform;
                    targetTransform = target.transform;
                }

                Vector3 originPos = attackerTransform.position;
                Vector3 movePos = (originPos + targetTransform.position) / 2f;

                attackerTransform.position = movePos;

                await UniTask.Delay(isSkill ? 1000 : 500);

                if (entity.IsDead)
                {
                    continue;
                }

                if (entity.IsPlayer == true)
                {
                    Monster targetMonster = targetTransform.GetComponent<Monster>();
                    entity.Hunter.AtkTarget(targetMonster);
                }
                else
                {
                    Character targetHunter = targetTransform.GetComponent<Character>();
                    entity.Mob.AtkTarget(targetHunter);
                }

                await UniTask.Delay(500);

                if (entity.IsDead == false)
                {
                    attackerTransform.position = originPos;
                }

                bool isHunterOrMonsterWipeOut = (playerParty.GetCurrentHunterCount() == 0) || (enemyParty.GetCurrentMonsterCount() == 0);
                
                if (isHunterOrMonsterWipeOut)
                {
                    break;
                }

                await UniTask.Delay(500);
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

    private int CompareActionOrder(BattleEntity a, BattleEntity b)
    {
        if (a.Speed != b.Speed)
        {
            return b.Speed.CompareTo(a.Speed);
        }
        
        if (a.IsPlayer != b.IsPlayer)
        {
            return a.IsPlayer ? -1 : 1;
        }

        return a.Index.CompareTo(b.Index);
    }

    private Monster FindMonsterTarget(int attackerIndex, MonsterParty enemyParty)
    {
        Monster mainTarget = enemyParty.GetMonster(attackerIndex);
        if ((mainTarget != null) && (mainTarget._isDead == false))
        {
            return mainTarget;
        }

        for (int i = 0; i < 3;  i++)
        {
            Monster subTarget = enemyParty.GetMonster(i);
            if ((subTarget != null) && (subTarget._isDead == false))
            {
                return subTarget;
            }
        }

        return null;
    }

    private Character FindHunterTarget(int attackerIndex, PlayerPartyController playerParty)
    {
        Character mainTarget = playerParty.GetHunter(attackerIndex);
        if ((mainTarget != null) && (mainTarget._isDead == false))
        {
            return mainTarget;
        }

        for (int i = 0; i < 3; i++)
        {
            Character subTarget = playerParty.GetHunter(i);
            if ((subTarget != null) && (subTarget._isDead == false))
            {
                return subTarget;
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