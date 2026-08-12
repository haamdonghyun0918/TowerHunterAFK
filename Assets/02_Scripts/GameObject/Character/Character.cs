using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Character : BattleCharacter
{
    [Header("스킬 관련")]
    private int _currentSkillCost;
    private int _maxSkillCost;
    private event Action _onSkillCostChange;

    [Header("데이터 관련")]
    private CharacterData _characterData;
    private string _characterId;

    [Header("전투 관련")]
    private Skill _skill;

    private Animator _characterAnimator;

    private bool _isSkillUsable = false;
    private void Awake()
    {
        if (_skill == null)
        {
            _skill = GetComponent<Skill>();
            if (_skill == null)
            {
                Debug.LogError("[Character] 스킬 컴포넌트를 가져오지 못했습니다.");
            }
        }
        BindOnSkillCostChanged(ConsoleOnSkillCostChanged);
    }

    private void OnEnable()    // [TODO] 우선 Start로 하고 동적생성이 되면 OnEnable로 변경
    {
        _currentSkillCost = 0;

        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log($"[Character] GameDataManager가 NULL입니다.");
        }
        //_characterData = GameDataManager.Instance.GetCharacterData("character_Test_01");     // [TODO] 하드코딩을 하지않고 (ID)데이터를 받아와야함
        _characterData = GameDataManager.Instance.GetData<CharacterData>("character_Test_01");
        _characterId = _characterData.Id;
        _maxSkillCost = _characterData.MaxSkillCost;
        InitializeSkill();
        SetStatData();
    }

    public string GetCharacterId()
    {
        return _characterId;
    }

    private int GetSkillDuration()
    {
        string skillId = _characterData.SkillId;
        var skillData = GameDataManager.Instance.GetData<SkillData>(skillId);
        int skillDuration = skillData.SkillDuration;
        return skillDuration;
    }

    private void InitializeSkill()
    {
        string skillId = _characterData.SkillId;

        if (_skill == null)
        {
            Debug.LogError($"[Character] 스킬 데이터를 불러오지 못했습니다.");
            return;
        }

        _skill.InitializeSkill(skillId);
    }

    private void SetStatData()
    {
        var baseStatData = GameDataManager.Instance.GetData<BaseStatData>(_characterData.BaseStatDataId);

        _characterAtk = baseStatData.BaseAtk;
        _characterAtkSpeed = baseStatData.BaseAtkSpeed;
        _characterMaxHp = baseStatData.BaseHp;
        _characterHp = baseStatData.BaseHp;
        _characterDefense = baseStatData.BaseDef;
    }

    private async UniTaskVoid UseSkill(Monster targetMonster)
    {
        int currentDamage = _characterAtk * _skill.GetSkillDamage();

        if (_isSkillUsable == true)
        {
            //[TODO] 스킬사용 모션
            _skill.UseSkillAsync().Forget();
            await UniTask.Delay(GetSkillDuration());
            if (_skill.GetSkillType() == SkillType.SelfTarget)
            {
                this.TakeDamage(currentDamage);
                Debug.Log($"[힐스킬] 타겟{this.name}에게 {-currentDamage} 힐을 줍니다.");
            }
            else
            {
                targetMonster.TakeDamage(currentDamage);
                Debug.Log($"[스킬공격] 타겟{targetMonster.name}에게 {currentDamage} 데미지를 줍니다.");
            }
        }
        CheckSkillUsable();
        InvokeCostChangedEvent();
    }

    public int AtkTarget(Monster targetMonster)
    {
        if (targetMonster._isDead == true) return 0;

        var targetMonsterTransform = targetMonster.gameObject.transform;
        _skill.SetSingleTargetTransform(targetMonsterTransform);

        IncreaseCurrentSkillCost(1);

        if (_isSkillUsable == false)
        {
            UseNormalAttack(targetMonster);
            return 1;
        }
        else
        {
            UseSkillCost(_skill.GetRequiredSkillCost());
            UseSkill(targetMonster).Forget();
            return 2;
        }
    }

    private void UseNormalAttack(Monster targetMonster)
    {
        //[TODO] 평타공격 모션
        targetMonster.TakeDamage(_characterAtk);
        Debug.Log($"[일반공격] 타겟{targetMonster.name}에게 {_characterAtk} 데미지를 줍니다.");
    }

    public void IncreaseCurrentSkillCost(int amount)
    {
        _currentSkillCost += amount;
        if (_currentSkillCost > _maxSkillCost)
        {
            _currentSkillCost = _maxSkillCost;
        }

        InvokeCostChangedEvent();
        CheckSkillUsable();
    }

    public void UseSkillCost(int amount)
    {
        if (_currentSkillCost < amount)
        {
            Debug.Log("스킬 코스트가 부족합니다!");
        }
        _currentSkillCost -= amount;
    }

    private void CheckSkillUsable()
    {
        int requiredSkillCost = _skill.GetRequiredSkillCost();

        if (requiredSkillCost <= _currentSkillCost)
        {
            _isSkillUsable = true;
        }
        else
        {
            _isSkillUsable = false;
        }
    }

    // 테스트용 치트 함수 =======================================================

    private void Update()   // 테스트용으로만 업데이트 사용
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            TestGetMaxSkillCost();
        }
    }

    private void TestGetMaxSkillCost()
    {
        _currentSkillCost = _maxSkillCost;
        CheckSkillUsable();
    }

    public void MakeFullHp()
    {
        _characterHp = _characterMaxHp;
        _isDead = false;
        gameObject.SetActive(true);
    }


    // 콘솔 띄우기용 이벤트 =========================================



    private void ConsoleOnSkillCostChanged()
    {
        Debug.Log($"현재 스킬 코스트: {_currentSkillCost}");
    }

    private void BindOnSkillCostChanged(Action skillCostChangedCallback)
    {
        _onSkillCostChange += skillCostChangedCallback;
    }

    private void InvokeCostChangedEvent()
    {
        _onSkillCostChange?.Invoke();
    }
}
