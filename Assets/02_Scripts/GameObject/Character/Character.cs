using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum NormalAttackType{
    None,
    Warrior,
    Wizard,
    Monk
}

public class Character : BattleCharacter
{
    [Header("스킬 관련")]
    private int _currentSkillCost;
    private int _maxSkillCost;

    //캐릭터 스탯 서비스에 현재 코스트와 최대 코스트를 전달하기 위함
    private event Action<int, int> _onSkillCostChange;
    public Transform _targetMonsterTransform { get; private set; }

    [Header("데이터 관련")]
    private CharacterData _characterData;
    private string _characterId;

    [Header("전투 관련")]
    private Skill _skill;

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

    private void OnEnable()
    {
        _currentSkillCost = 0;

        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log($"[Character] GameDataManager가 NULL입니다.");
        }
    }

    public void InitCharacter(CharacterData characterData)
    {
        _characterData = characterData;

        if (_characterData == null)
        {
            Debug.LogError("[Character]데이터가 Null입니다.");
            return;
        }

        _characterId = _characterData.Id;
        _maxSkillCost = _characterData.MaxSkillCost;

        InitializeSkill();
        SetStatData();
    }

    public string GetCharacterId()
    {
        return _characterId;
    }

    //추가
    public int GetCurrentSkillCost()
    {
        return _currentSkillCost;
    }

    public int GetMaxSkillCost()
    {
        return _maxSkillCost;
    }
    //끝

    public int GetSkillDuration()
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

    private async UniTask UseSkill(Monster targetMonster)
    {
        SetSingleTargetTransform(targetMonster);

        int currentDamage = _characterAtk * _skill.GetSkillDamage();

        if (_isSkillUsable == true)
        {
            ChangeState(CharacterState.SkillAttack);
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
        CheckSkillUseable();
        InvokeCostChangedEvent();
    }

    public async UniTask AtkTarget(Monster targetMonster)
    {

        if (targetMonster._isDead == true) return;

        var characterType = _characterData.CharacterType;

        IncreaseCurrentSkillCost(1);

        if (_isSkillUsable == false)
        {
            await UseNormalAttack(targetMonster);
            Debug.Log($"[일반공격] 타겟{targetMonster.name}에게 {_characterAtk} 데미지를 줍니다.");
        }
        else
        {
            UseSkillCost(_skill.GetRequiredSkillCost());
            await UseSkill(targetMonster);
        }

        ChangeState(CharacterState.Idle);
    }

    private void SetSingleTargetTransform(Monster targetMonster)
    {
        if (_skill.GetSkillType() != SkillType.SingleTarget && _skill.GetSkillType() != SkillType.MultiTarget) return;
        var singleTargetTransform = targetMonster.gameObject.transform;
        _skill.SetSingleTargetTransform(singleTargetTransform);
    }

    private async UniTask UseNormalAttack(Monster targetMonster)
    {
        ChangeState(CharacterState.NormalAttack);
        var characterType = _characterData.CharacterType;
        await UniTask.Delay(GetNormalAttackMotionDuration(SetCharacterType(characterType)));
        targetMonster.TakeDamage(_characterAtk);
    }

    public void IncreaseCurrentSkillCost(int amount)
    {
        _currentSkillCost += amount;
        if (_currentSkillCost > _maxSkillCost)
        {
            _currentSkillCost = _maxSkillCost;
        }

        InvokeCostChangedEvent();
        CheckSkillUseable();
    }

    public void UseSkillCost(int amount)
    {
        if (_currentSkillCost < amount)
        {
            Debug.Log("스킬 코스트가 부족합니다!");
        }
        _currentSkillCost -= amount;

        InvokeCostChangedEvent();
    }

    private void CheckSkillUseable()
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

    private int GetNormalAttackMotionDuration(NormalAttackType newType)
    {
        switch (newType)
        {
            case NormalAttackType.None:
                {
                    return 0;
                }
            case NormalAttackType.Warrior:
                {
                    return 1000;
                }
            case NormalAttackType.Wizard:
                {
                    return 1500;
                }
            case NormalAttackType.Monk:
                {
                    return 1000;
                }
            default:
                {
                    return 1000;
                }
        }
    }

    private NormalAttackType SetCharacterType(string characterType)
    {
        if (characterType == null) return NormalAttackType.None;
        characterType = _characterData.CharacterType;

        try
        {
            return (NormalAttackType)System.Enum.Parse(typeof(NormalAttackType), characterType, true);
        }

        catch
        {
            Debug.LogError($"[Character] {characterType}캐릭터타입 변환 실패");
            return NormalAttackType.None;
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
        CheckSkillUseable();
        //추가
        InvokeCostChangedEvent();
    }

    public void MakeFullHp()
    {
        _characterHp = _characterMaxHp;
        _isDead = false;
        gameObject.SetActive(true);
        //추가
        InvokeStatChangedEvent();
        InvokeCostChangedEvent();
    }


    // 콘솔 띄우기용 이벤트 =========================================


    //현재 코스트와 최대 코스트 받기
    private void ConsoleOnSkillCostChanged(int currentSkillCost, int maxSkillCost)
    {
        Debug.Log($"{_characterId}현재 스킬 코스트: {currentSkillCost}");
    }

    //Action -> Action<int,int>
    private void BindOnSkillCostChanged(Action<int, int> skillCostChangedCallback)
    {
        _onSkillCostChange += skillCostChangedCallback;
    }

    public void BindOnSkillCostChangedEvent(Action<int, int> skillCostChangedEventCallback)
    {
        _onSkillCostChange += skillCostChangedEventCallback;
    }

    public void UnbindOnSkillCostChangedEvent(Action<int,int> skillCostChangedEventCallBack)
    {
        _onSkillCostChange -= skillCostChangedEventCallBack;
    }

    private void InvokeCostChangedEvent()
    {
        _onSkillCostChange?.Invoke(_currentSkillCost, _maxSkillCost);
    }
}
