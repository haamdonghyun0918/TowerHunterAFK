using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("스탯 관련")]
    private int _monsterHp;
    private int _monsterMp;
    private int _monsterAtkSpeed;

    private Animator _monsterAnimator;

    private Action<int> _onChangedHp;

    private void Awake()
    {
        //[TODO] 데이터 불러오기
    }

    private void OnEnable()
    {
        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
    }

    private void AtkTarget()
    {
        //[TODO] 평타공격
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
    }

    private void ChangeState()
    {
        //[TODO] 애니메이션 변경 (평타공격, 죽는애니메이션)
    }

    private void ResetState()
    {
        //[TODO] 스탯리셋
    }

    private void OnChangeHP(int monsterHp)
    {
        _monsterHp = monsterHp;
    }
}
