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

    private Animator _monsterAnimator;

    private Action<int, int> _onChangedHp;

    private void Awake()
    {
        this.gameObject.SetActive(true);
    }

    private void Start()    // [TODO] 우선 Start로 하고 동적생성이 되면 OnEnable로 변경
    {
        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log("[Monster] GameDataManager가 NULL입니다.");
        }
        _monsterData = GameDataManager.Instance.GetMonsterData("monster_Test_01");  // [TODO] 하드코딩을 하지않고 (ID)데이터를 받아와야함
        SetStatData();
    }

    private void SetStatData()
    {
        _monsterAtk = _monsterData.BaseAtk;
        _monsterAtkSpeed = _monsterData.BaseAtkSpeed;
        _monsterHp = _monsterData.BaseHp;
        _monsterMaxHp = _monsterData.BaseHp;
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

    public void TakeDamage(int damage)
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