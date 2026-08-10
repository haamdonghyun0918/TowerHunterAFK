using System;
using Unity.VisualScripting;
using UnityEngine;

public class Character : BattleCharacter
{
    [Header("스킬 관련")]
    [SerializeField] private Collider Collider_Skill;
    [SerializeField] private GameObject Prefab_Skill;
    [SerializeField] private Transform Root_SkillSpawn;

    [Header("데이터 관련")]
    private CharacterData _characterData;
    private string _characterId;

    [Header("전투 관련")]
    [SerializeField] private GameObject TargetMonster;
    private Skill _skill;

    private Animator _characterAnimator;

    

    private bool _isSkillUsable = false;
    private void Awake()
    {

    }

    private void OnEnable()    // [TODO] 우선 Start로 하고 동적생성이 되면 OnEnable로 변경
    {

        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log($"[Character] GameDataManager가 NULL입니다.");
        }
        //_characterData = GameDataManager.Instance.GetCharacterData("character_Test_01");     // [TODO] 하드코딩을 하지않고 (ID)데이터를 받아와야함
        _characterData = GameDataManager.Instance.GetData<CharacterData>("character_Test_01");
        _characterId = _characterData.Id;
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
    }

    private void UseSkill()
    {
        string skillId = _characterData.SkillId;
        _skill.SetSkillId(skillId);


        if (_isSkillUsable == true)
        {
            _skill.UseSkill();
        }
    }

    public void AtkTarget(Monster targetMonster)
    {
        if (targetMonster._isDead == true) return;

        if (_isSkillUsable == false)
        {
            Debug.Log($"타겟{targetMonster.name}에게 {_characterAtk} 데미지를 줍니다.");
            //[TODO] 평타공격 모션
            targetMonster.TakeDamage(_characterAtk);
        }
    }
}
