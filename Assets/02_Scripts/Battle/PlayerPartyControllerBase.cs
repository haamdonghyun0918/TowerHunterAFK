using UnityEngine;

public class PlayerPartyControllerBase : MonoBehaviour
{
    [SerializeField] protected Transform[] _playerSlots;
    protected Character[] _hunters;

    [SerializeField] protected GameObject _partyCamera;

    protected virtual void Awake()
    {
        _hunters = new Character[_playerSlots.Length];
    }

    public void SetCameraActive(bool isActive)
    {
        if (_partyCamera != null)
        {
            _partyCamera.SetActive(isActive);
        }
    }

    public int MaxPartySize => _hunters.Length;

    public Character GetHunter(int index)
    {
        if (index >= 0 && index < _hunters.Length)
        {
            return _hunters[index];
        }

        return null;
    }

    public int GetCurrentHunterCount()
    {
        int count = 0;
        foreach (var hunter in _hunters)
        {
            if (hunter != null && !hunter._isDead)
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

    public void MakeFullHPAllHunters()
    {
        foreach (var hunter in _hunters)
        {
            if (hunter != null)
            {
                Debug.Log($"[PlayerPartyController] MakeFullHPAllHunters: {hunter.name}의 HP를 회복합니다.");
                hunter.MakeFullHp();
            }
        }
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
}
