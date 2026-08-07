using System.Collections.Generic;
using UnityEngine;

public class PlayerPartyController : MonoBehaviour
{
    private float moveSpeed = 5.0f;

    public bool _isBattling = false;
    public bool _isMovable = false;

    public Transform[] _playerSlots = new Transform[3];
    private Character[] _hunters = new Character[3];

    
    private void Update()
    {
        if ((_isBattling == false) && (_isMovable == true))
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }
    }

    public Character GetHunter(int index)
    {
        if ((index >= 0) && (index < _hunters.Length))
        {
            return _hunters[index];
        }
        else
        {
            Debug.LogError($"[PlayerPartyController] GetHunter: index {index}는 범위를 초과함!");
            return null;
        }
    }

    public int GetCurrentHunterCount()
    {
        int count = 0;

        foreach (var hunter in _hunters)
        {
            if (hunter != null)
            {
                count++;
            }
        }

        return count;
    }

    public bool AddHunter(Character newHunter)
    {
        if (newHunter == null)
        {
            Debug.LogError($"[PlayerPartyController] AddHunter: newHunter가 NULL입니다.");
            return false;
        }

        for (int i = 0; i < _hunters.Length; i++)
        {
            if (_hunters[i] == null)
            {
                _hunters[i] = newHunter;
                newHunter.transform.SetParent(_playerSlots[i]);
                newHunter.transform.localPosition = Vector3.zero;
                newHunter.transform.localRotation = Quaternion.identity;
                //newHunter.OnHunterDeath += HandleHunterDeath;

                return true;
            }
        }

        return false;
    }

    public void HandleHunterDeath(Character deadHunter)
    {
        for (int i = 0; i < _hunters.Length; i++)
        {
            if (_hunters[i] == deadHunter)
            {
                _hunters[i] = null;
                //newHunter.OnHunterDeath -= HandleHunterDeath;
                break;
            }
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
