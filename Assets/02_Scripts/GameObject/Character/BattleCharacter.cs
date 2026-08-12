using System;
using UnityEngine;

public class BattleCharacter : MonoBehaviour
{
    [Header("공통스탯")]
    protected int _characterHp;
    protected int _characterMaxHp;
    protected int _characterAtk;
    protected int _characterDefense;
    public int _characterAtkSpeed { get; protected set; }
    public bool _isDead { get; protected set; }
    public string _instanceId { get; set; }

    private Action<int, int> _onChangedHp;

    private void OnEnable()
    {
        _isDead = false;
    }

    private void OnDisable()
    {
        _isDead = true;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead == true) return;

        int currentDamage = ApplyDefenseDamage(damage);

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
        //[TODO] 죽음 애니메이션 재생
        ResetStateChangedEvent();
        this.gameObject.SetActive(false);
        _isDead = true;
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
        _onChangedHp?.Invoke(_characterHp, _characterMaxHp);
    }

    private void ChangeState()
    {
        //[TODO] 애니메이션 변경 (스킬사용, 평타공격, 죽는애니메이션)
    }
}