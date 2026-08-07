using System.Collections.Generic;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    public Transform[] _monsterSlots = new Transform[3];
    private Monster[] _monsters = new Monster[3];

    public Monster GetMonster(int index)
    {
        if ((index >= 0) && (index < _monsters.Length))
        {
            return _monsters[index];
        }
        else
        {
            Debug.LogError($"[MonsterParty] GetMonster: index {index}는 범위를 초과함!");
            return null;
        }
    }

    public int GetCurrentMonsterCount()
    {
        int count = 0;

        foreach (var monster in _monsters)
        {
            if (monster != null)
            {
                count++;
            }
        }

        return count;
    }

    public bool AddMonster(Monster newMonster)
    {
        if (newMonster == null)
        {
            Debug.LogError($"[MonsterParty] AddMonster: newMonster가 NULL입니다.");
            return false;
        }

        for (int i = 0; i < _monsters.Length; i++)
        {
            if (_monsters[i] == null)
            {
                _monsters[i] = newMonster;
                newMonster.transform.SetParent(_monsterSlots[i]);
                newMonster.transform.localPosition = Vector3.zero;
                newMonster.transform.localRotation = Quaternion.identity;
                //newMonster.OnMonsterDeath += HandleMonsterDeath;

                return true;
            }
        }

        return false;
    }

    public void HandleMonsterDeath(Monster deadMonster)
    {
        for (int i = 0; i < _monsters.Length; i++)
        {
            if (_monsters[i] == deadMonster)
            {
                _monsters[i] = null;
                //newMonster.OnMonsterDeath -= HandleMonsterDeath;
                break;
            }
        }

        if (GetCurrentMonsterCount() == 0)
        {
            Debug.Log("모든 몬스터가 리타이어했습니다. 전투에서 승리했습니다.");
            BattleManager.Instance.EndBattle(BattleManager.Instance._playerParty, this.gameObject);
        }
    }
}
