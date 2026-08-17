using System;
using UnityEngine;

public enum CharacterState
{
    None,
    Die,
    Idle,
    Run,
    NormalAttack,
    SkillAttack
}

public class BattleCharacter : MonoBehaviour
{
    [Header("공통스탯")]
    protected int _characterHp;
    protected int _characterMaxHp;
    protected int _characterAtk;
    protected int _characterDefense;

    [Header("애니메이터")]
    [SerializeField] private Animator _characterAnimator;

    public int _characterAtkSpeed { get; protected set; }
    public bool _isDead { get; protected set; }
    public string _instanceId { get; set; }


    private Action<int, int> _onChangedHp;

    private void OnEnable()
    {
        ChangeState(CharacterState.Idle);
        _isDead = false;
    }

    private void OnDisable()
    {
        _isDead = true;
    }

    private void OnDestroy()
    {
        ResetStateChangedEvent();
    }

    public void TakeDamage(int damage)
    {
        if (_isDead == true) return;

        int currentDamage = ApplyDefenseDamage(damage);

        //[TODO] ChangeState(CharacterState.Hit);

        _characterHp -= currentDamage;
        
        if (_characterHp <= 0)
        {
            _characterHp = 0;
        }

        InvokeStatChangedEvent();

        if (_characterHp <= 0)
        {
            Die();
        }
        else
        {
            ChangeState(CharacterState.Idle);
        }
    }

    private int ApplyDefenseDamage(int damage)
    {
        int currentDamage = 0;

        if (_characterDefense < damage)
        {
            currentDamage = damage - _characterDefense;
        }

        return currentDamage;
    }

    private void Die()
    {
        ChangeState(CharacterState.Die);

        _isDead = true;

        if (this != null && this.gameObject != null)
        {
            this.gameObject.SetActive(false);
        }
    }

    //추가
    public int GetCurrentHp()
    {
        return _characterHp;
    }

    public int GetMaxHp()
    {
        return _characterMaxHp;
    }

    public void UnbindOnStatChangedEvent(Action<int,int> hpChangeCallback)
    {
        _onChangedHp -= hpChangeCallback;
    }
    //끝

    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback)
    {
        _onChangedHp += hpChangeCallback;
    }

    private void ResetStateChangedEvent()
    {
        _onChangedHp = null;
    }

    //Character.MakeFullHp()에서 호출할 수 있도록
    protected void InvokeStatChangedEvent()
    {
        _onChangedHp?.Invoke(_characterHp, _characterMaxHp);
    }

    protected void ChangeState(CharacterState newState)
    {
        switch (newState)
        {
            case CharacterState.Idle:
                {
                    ResetAllAnimatorParameters();
                }
                break;
            case CharacterState.NormalAttack:
                {
                    _characterAnimator.SetBool("IsNormalAttack", true);
                }
                break;
            case CharacterState.SkillAttack:
                {
                    _characterAnimator.SetBool("IsSkillAttack", true);
                }
                break;
            case CharacterState.Die:
                {
                    _characterAnimator.SetBool("IsDead", true);
                }
                break;
        }
    }

    private void ResetAllAnimatorParameters()
    {
        _characterAnimator.SetBool("IsNormalAttack", false);
        _characterAnimator.SetBool("IsSkillAttack", false);
        _characterAnimator.SetBool("IsDead", false);
    }
}