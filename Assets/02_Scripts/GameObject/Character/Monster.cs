using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("스탯 관련")]
    private int _monsterMaxHp;
    private int _monsterHp;
    private int _monsterMp;
    private int _monsterAtkSpeed;
    private int _monsterAtk;

    private Animator _monsterAnimator;

    private Action<int, int> _onChangedHp;

    private void Awake()
    {
        //[TODO] 데이터 불러오기
        _monsterHp = 100;
        _monsterAtkSpeed = 10;
        _monsterAtk = 30;
        this.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
    }

    private void AtkTarget(string targetId)
    {
        //[TODO] 평타공격
        // var data = GameDataManager.Inst.GetData(targetId);
        if (targetId == "data.Id")  // 포멧으로 데이터 확인
        {
            // var target = GameObjectManager.Inst.GetTarget(targetId);
            // target.TakeDamage(_monsterAtk);
        }
    }

    private void TakeDamage(int damage)
    {
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