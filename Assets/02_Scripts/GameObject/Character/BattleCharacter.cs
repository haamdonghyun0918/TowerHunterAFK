using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public enum CharacterState
{
    None,
    Die,
    Idle,
    Run,
    NormalAttack,
    SkillAttack,
    Hit
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

    [Header("피격 이펙트")]
    [SerializeField] private HitFlashEffect _hitFlashEffect;

    [Header("피격 데미지 텍스트")]
    [SerializeField] private DamageTextEffect _damageTextEffect;

    private CancellationTokenSource _hitCancellationTokenSource;

    public int _characterAtkSpeed { get; protected set; }
    public bool _isDead { get; protected set; }
    public string _instanceId { get; set; }

    public bool _isRunning;

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

    public async UniTask TakeDamage(int damage)
    {
        if (_isDead == true) return;

        int currentDamage = ApplyDefenseDamage(damage);

        _hitCancellationTokenSource?.Cancel();
        _hitCancellationTokenSource = new CancellationTokenSource();

        ChangeState(CharacterState.Hit);

        _hitFlashEffect?.PlayHitFlash();

        if (currentDamage > 0)
        {
            _damageTextEffect?.ShowDamage(currentDamage);
        }

        _characterHp -= currentDamage;
        
        if (_characterHp <= 0)
        {
            _characterHp = 0;
        }

        InvokeStatChangedEvent();

        if (_characterHp <= 0)
        {
            Die().Forget();
        }

        else
        {
            try
            {
                await UniTask.Delay(500, cancellationToken: _hitCancellationTokenSource.Token);
                ChangeState(CharacterState.Idle);
            }

            catch(OperationCanceledException)
            {

            }
        }
    }

    private int ApplyDefenseDamage(int damage)
    {
        int currentDamage = 0;

        if (_characterDefense < damage)
        {
            currentDamage = damage - _characterDefense;
        }

        if (currentDamage <= 0)
        {
            currentDamage = 1;
        }

        return currentDamage;
    }

    private async UniTask Die()
    {
        ChangeState(CharacterState.Die);

        await UniTask.Delay(500);

        _isDead = true;

        if (this != null && this.gameObject != null)
        {
            this.gameObject.SetActive(false);
        }
    }

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

    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback)
    {
        _onChangedHp += hpChangeCallback;
    }

    private void ResetStateChangedEvent()
    {
        _onChangedHp = null;
    }

    protected void InvokeStatChangedEvent()
    {
        _onChangedHp?.Invoke(_characterHp, _characterMaxHp);
    }

    public void ChangeState(CharacterState newState)
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
            case CharacterState.Hit:
                {
                    _characterAnimator.SetBool("IsDamaged", true);
                }
                break;
            case CharacterState.Run:
                {
                    _characterAnimator.SetBool("IsMoved", true);
                }
                break;
        }
    }

    private void ResetAllAnimatorParameters()
    {
        _characterAnimator.SetBool("IsNormalAttack", false);
        _characterAnimator.SetBool("IsSkillAttack", false);
        _characterAnimator.SetBool("IsDead", false);
        _characterAnimator.SetBool("IsDamaged", false);
        _characterAnimator.SetBool("IsMoved", false);
        _isRunning = false;
    }
}