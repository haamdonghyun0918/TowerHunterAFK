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
            if ((monster != null) && (monster._isDead == false))
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
                _monsters[i].transform.SetParent(_monsterSlots[i]);
                _monsters[i].transform.localPosition = Vector3.zero;
                _monsters[i].transform.localRotation = Quaternion.identity;
                //_monsters[i].OnMonsterDeath += HandleMonsterDeath;

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
    }

    public void ClearParty()
    {
        for (int i = 0; i < _monsters.Length; i++)
        {
            _monsters[i] = null;
        }
    }
}
