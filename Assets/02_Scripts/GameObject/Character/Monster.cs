using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("스탯 관련")]
    private int _monsterMaxHp;
    private int _monsterHp;
    private int _monsterAtkSpeed;
    private int _monsterAtk;

    [Header("데이터 관련")]
    private MonsterData _monsterData;

    [Header("전투 관련")]
    [SerializeField] private GameObject TargetCharacter;
    private Character _targetCharacterComponent;

    public bool _isDead { get; private set; }

    private Animator _monsterAnimator;

    private Action<int, int> _onChangedHp;

    private void Awake()
    {
        this.gameObject.SetActive(true);
        _targetCharacterComponent = TargetCharacter.GetComponentInChildren<Character>();
    }

    private void OnEnable()
    {
        _isDead = false;

        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log("[Monster] GameDataManager가 NULL입니다.");
        }

        _monsterData = GameDataManager.Instance.GetData<MonsterData>("monster_Test_01");    // [TODO] 하드코딩을 하지않고 (ID)데이터를 받아와야함
        SetStatData();
    }

    private void SetStatData()
    {
        _monsterAtk = _monsterData.BaseAtk;
        _monsterAtkSpeed = _monsterData.BaseAtkSpeed;
        _monsterHp = _monsterData.BaseHp;
        _monsterMaxHp = _monsterData.BaseHp;
    }

    public void AtkTarget(Character TargetCharacter)
    {
        if (TargetCharacter._isDead == true) return;

        TargetCharacter.TakeDamage(_monsterAtk);
    }

    public void TakeDamage(int damage)
    {
        if (_isDead == true) return;

        _monsterHp -= damage;

        if (_monsterHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //[TODO] 죽음 애니메이션 재생
        this.gameObject.SetActive(false);
        _isDead = true;
    }

    private void ChangeState()
    {
        //[TODO] 애니메이션 변경 (평타공격, 죽는애니메이션)
    }

    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback)
    {
        _onChangedHp += hpChangeCallback;
    }

    private void ResetStateChangedEvent()
    {
        _onChangedHp = null;
    }

    private void InvokeStatChangedEvent()
    {
        _onChangedHp?.Invoke(_monsterHp, _monsterMaxHp);
    }
}