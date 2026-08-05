using System;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("스킬 관련")]
    [SerializeField] private Collider Collider_Skill;
    [SerializeField] private GameObject Prefab_Skill;
    [SerializeField] private Transform Root_SkillSpawn;

    [Header("스탯 관련")]
    private int _characterHp;
    private int _characterMp;
    private int _characterAtkSpeed;

    private Animator _characterAnimator;

    private Action<int> _onChangedHp;
    private Action<int> _onChangedMp;

    private bool _isCoolTime;

    private void Awake()
    {
        //[TODO] 데이터 불러오기
    }

    private void OnEnable()
    {
        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
    }

    private void UseSkill()
    {
        if (_isCoolTime == false)
        {
            //[TODO] 스킬사용
        }
    }

    private void AtkTarget()
    {
        if (_isCoolTime == true)
        {
            //[TODO] 평타공격
        }
    }

    private void TakeDamage(int damage)
    {
        _characterHp -= damage;
        
        if (_characterHp <= 0)
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
        //[TODO] 애니메이션 변경 (스킬사용, 평타공격, 죽는애니메이션)
    }

    private void ResetState()
    {
        //[TODO] 스탯리셋
    }

    private void OnChangeHP(int characterHp)
    {
        _characterHp = characterHp;
    }

    private void OnChangeMP(int characterMp)
    {
        _characterMp = characterMp;
    }
}
