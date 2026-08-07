using Cysharp.Threading.Tasks;
using NUnit.Framework;
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
                    Speed = monster._monsterAtkSpeed,
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

                //공격을 하는 aud령을 배틀 매니저가 내리느냐, 캐릭터/몬스터가 내리느냐 하는 생각이 필요하다. 
                //어차피 공격 메서드를 캐릭터에서 사용해도 배틀매니저에 공격 요청을 보내니까 그냥 여기서 처리하면 되는거 아닌가?
                //상의하고 배틀 매니저에서 처리할지, 캐닉터/몬트서에서 처리할지(타깃 정보를 넘겨줌) 결정필요.
                //일단은 그냥 객체를 던지는 것으로 구현(
                if (entity.IsPlayer == true)
                {
                    Monster target = FindMonsterTarget(entity.Index, enemyParty);
                    if (target != null)
                    {
                        entity.Hunter.AtkTarget(target);
                    }
                }
                else
                {
                    Character target = FindHunterTarget(entity.Index, playerParty);
                    if (target != null)
                    {
                        entity.Mob.AtkTarget(target);
                    }
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

    //헌터 파티 승리/패배로 나누는게 좋을 듯.
    public void EndBattle(PlayerPartyController playerParty, GameObject monsterParty)
    {
        Debug.Log("전투 종료!");
        monsterParty.SetActive(false);
        playerParty._isBattling = false;
        playerParty._isMovable = true;
    }

}