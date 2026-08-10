using System;
using Unity.VisualScripting;
using UnityEngine;

public class Character : BattleCharacter
{
    [Header("스킬 관련")]
    [SerializeField] private Collider Collider_Skill;
    [SerializeField] private GameObject Prefab_Skill;
    [SerializeField] private Transform Root_SkillSpawn;

    [Header("스탯 관련")]
    private int _characterMp;
    private int _characterMaxMp;

    [Header("데이터 관련")]
    private CharacterData _characterData;
    private string _characterId;

    [Header("전투 관련")]
    [SerializeField] private GameObject TargetMonster;
    [SerializeField] private Monster _targetMonsterComponent;

    private Animator _characterAnimator;

    private Action<int, int> _onChangedHp;
    private Action<int, int> _onChangedMp;

    private bool _isSkillUsable = false;
    public bool _isDead { get; private set; }

    private void Awake()
    {

    }

    private void OnEnable()    // [TODO] 우선 Start로 하고 동적생성이 되면 OnEnable로 변경
    {
        _isDead = false;

        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log($"[Character] GameDataManager가 NULL입니다.");
        }
        //_characterData = GameDataManager.Instance.GetCharacterData("character_Test_01");     // [TODO] 하드코딩을 하지않고 (ID)데이터를 받아와야함
        _characterData = GameDataManager.Instance.GetData<CharacterData>("character_Test_01");
        _characterId = _characterData.Id;
        _targetMonsterComponent = TargetMonster.GetComponentInChildren<Monster>();    // [TODO] 타겟 몬스터 정하는 방식 정해야함
        SetStatData();
    }

    public string GetCharacterId()
    {
        return _characterId;
    }

    private void SetStatData()
    {
        var baseStatData = GameDataManager.Instance.GetData<BaseStatData>(_characterData.BaseStatDataId);

        _characterAtk = baseStatData.BaseAtk;
        _characterAtkSpeed = baseStatData.BaseAtkSpeed;
        _characterMaxHp = baseStatData.BaseHp;
        _characterHp = baseStatData.BaseHp;
        _characterMaxMp = baseStatData.BaseMp;
        _characterMp = baseStatData.BaseMp;
    }

    private void UseSkill()
    {
        string skillId = _characterData.SkillId;
        if (_isSkillUsable == true)
        {
            //[TODO] 스킬사용 모션
            //UseSkill(skillId);
        }
    }

    public void AtkTarget(Monster targetMonster)
    {
        if (targetMonster._isDead == true) return;

        if (_isSkillUsable == false)
        {
            Debug.Log($"타겟에게 {_characterAtk} 데미지를 줍니다.");
            //[TODO] 평타공격 모션
            targetMonster.TakeDamage(_characterAtk);
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
