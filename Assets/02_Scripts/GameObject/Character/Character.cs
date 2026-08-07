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
    private int _characterMaxHp;
    private int _characterMp;
    private int _characterMaxMp;
    private int _characterAtk;
    private int _characterAtkSpeed;

    [Header("데이터 관련")]
    private CharacterData _characterData;

    [Header("전투 관련")]
    [SerializeField] private GameObject TargetMonster;
    [SerializeField] private Monster _targetMonsterComponent;

    private Animator _characterAnimator;

    private Action<int, int> _onChangedHp;
    private Action<int, int> _onChangedMp;

    private bool _isCoolTime = false;
    private bool _isDead = false;

    private void Awake()
    {
        _isDead = false;
    }

    private void Start()    // [TODO] 우선 Start로 하고 동적생성이 되면 OnEnable로 변경
    {
        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log($"[Character] GameDataManager가 NULL입니다.");
        }
        _characterData = GameDataManager.Instance.GetCharacterData("character_Test_01");     // [TODO] 하드코딩을 하지않고 (ID)데이터를 받아와야함
        _targetMonsterComponent = TargetMonster.GetComponentInChildren<Monster>();    // [TODO] 타겟 몬스터 정하는 방식 정해야함
        SetStatData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AtkTarget();
        }
    }

    private void SetStatData()
    {
        var baseStatData = GameDataManager.Instance.GetBaseStatData(_characterData.BaseStatDataId);

        _characterAtk = baseStatData.BaseAtk;
        _characterAtkSpeed = baseStatData.BaseAtkSpeed;
        _characterMaxHp = baseStatData.BaseHp;
        _characterHp = baseStatData.BaseHp;
        _characterMaxMp = baseStatData.BaseMp;
        _characterMp = baseStatData.BaseMp;
    }

    private void UseSkill()
    {
        string skillId = _characterData.Skill;
        if (_isCoolTime == false) 
        {
            //[TODO] 스킬사용
            //UseSkill(skillId);
        }
    }

    private void AtkTarget()
    {
        if (_isDead == true) return;

        _targetMonsterComponent.TakeDamage(_characterAtk);
        Debug.Log($"타겟에게 {_characterAtk} 데미지를 줍니다.");
        if (_isCoolTime == true)
        {
            //[TODO] 평타공격
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDead == true) return;

        _characterHp -= damage;
        InvokeStatChangedEvent();
        Debug.Log($"{_characterData.Name}가 {damage} 데미지를 받았습니다.");
        
        if (_characterHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //[TODO] 죽음 애니메이션 재생
        Debug.Log($"{_characterData.Name}이 죽었습니다.");
        ResetStateChangedEvent();
        this.gameObject.SetActive( false );
        _isDead = true;
    }

    private void ChangeState()
    {
        //[TODO] 애니메이션 변경 (스킬사용, 평타공격, 죽는애니메이션)
    }

    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback, Action<int, int> mpChangeCallback)
    {
        _onChangedHp += hpChangeCallback;
        _onChangedMp += mpChangeCallback;
    }

    private void ResetStateChangedEvent()
    {
        _onChangedHp = null;
        _onChangedMp = null;
    }

    private void InvokeStatChangedEvent()
    {
        _onChangedHp?.Invoke(_characterHp, _characterMaxHp);
        _onChangedMp?.Invoke(_characterMp, _characterMaxMp);
    }
}
