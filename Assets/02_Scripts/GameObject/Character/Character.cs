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
    CharacterLevelManager _characterLevelManager = new CharacterLevelManager();

    [Header("스킬 관련")]
    private int _currentSkillCost;
    private int _maxSkillCost;

    //캐릭터 스탯 서비스에 현재 코스트와 최대 코스트를 전달하기 위함
    private event Action<int, int> _onSkillCostChange;
    private event Action<int> _onCharacterLevelChange;
    public Transform _targetMonsterTransform { get; private set; }

    [Header("데이터 관련")]
    private CharacterData _characterData;
    private string _characterId;
    private int _characterCurrentExp;

    private string _characterUniqueId;
    private int _characterLevel;
    private int _needExpForLevelUp = 2000;
    private int _maxLevelForRarity;

    private int _characterEnhancement;

    public bool _isRunning;

    [Header("전투 관련")]
    private Skill _skill;

    private bool _isEquipmentEventBound;

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
    private void OnDestroy()
    {
        UnbindEquipmentChangedEvent(); 
    }

    public void InitCharacter(CharacterData characterData, string characterUniqueId, bool isBossRaid = false)
    {
        _characterData = characterData;
        _characterUniqueId = characterUniqueId;

        if (_characterData == null)
        {
            Debug.LogError("[Character]데이터가 Null입니다.");
            return;
        }

        _characterId = _characterData.Id;
        _maxSkillCost = _characterData.MaxSkillCost;

        if (SaveManager.Instance != null && SaveManager.Instance.CharacterDict.TryGetValue(characterUniqueId, out var saveData))
        {
            _characterEnhancement = saveData.Rank;
            _characterCurrentExp = (int)saveData.Exp;
            _characterLevel = 1 + (_characterCurrentExp / _needExpForLevelUp);
        }

        else
        {
            _characterEnhancement = 0;
            _characterCurrentExp = 0;
            _characterLevel = 1;
        }

        InitializeSkill();
        SetStatData(true);

        if (isBossRaid == true)
        {
            _characterHp = _characterMaxHp;
            _isDead = false;
            this.gameObject.SetActive(true);
        }
        else
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CharacterDict.TryGetValue(characterUniqueId, out var saveDataForHp))
            {
                if (saveDataForHp.CurrentHP != -1)
                {
                    _characterHp = Mathf.Clamp(saveDataForHp.CurrentHP, 0, _characterMaxHp);

                    if (_characterHp <= 0)
                    {
                        _isDead = true;
                        this.gameObject.SetActive(false);
                    }
                    else
                    {
                        _isDead = false;
                        this.gameObject.SetActive(true);
                    }
                }
                else
                {
                    saveDataForHp.CurrentHP = _characterMaxHp;
                }
            }

            BindEquipmentChangedEvent();
        }
    }

    public string GetCharacterId()
    {
        return _characterId;
    }

    public string GetCharacterName()
    {
        if( _characterData == null)
        {
            Debug.LogWarning("[Character] 캐릭터 데이터가 없어 이름을 가져올 수 없습니다.");
            return "";
        }

        return _characterData.Name;
    }

    //추가
    public string GetCharacterUniqueId()
    {
        return _characterUniqueId;
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

    //추가
    public int GetCharacterCurHP()
    {
        return _characterHp;
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

    private void SetStatData(bool restoreHp)
    {
        BaseStatData baseStatData = GameDataManager.Instance.GetData<BaseStatData>(_characterData.BaseStatDataId);

        if (baseStatData == null)
        {
            Debug.LogError("[Character] BaseStatData가 없습니다.");
            return;
        }

        EquipmentStatBonus equipmentBonus = new EquipmentStatBonus();

        if (NetworkManager.Instance != null && NetworkManager.Instance.EquipmentService != null)
        {
            equipmentBonus = NetworkManager.Instance.EquipmentService.GetCharacterEquipmentStatBonus(_characterUniqueId);
        }

        int previousMaxHp = _characterMaxHp;
        int previousHp = _characterHp;

        int levelBonusAtk = _characterData.AtkPerLevel * (_characterLevel - 1);
        int levelBonusHp = _characterData.HpPerLevel * (_characterLevel - 1);
        int levelBonusDef = _characterData.DefPerLevel * (_characterLevel - 1);

        _characterAtk = baseStatData.BaseAtk + equipmentBonus.Atk + levelBonusAtk;
        _characterAtkSpeed = baseStatData.BaseAtkSpeed + equipmentBonus.AtkSpeed;
        _characterMaxHp = baseStatData.BaseHp + equipmentBonus.Hp + levelBonusHp;
        _characterDefense = baseStatData.BaseDef + equipmentBonus.Def + levelBonusDef;

        if (restoreHp || previousMaxHp <= 0)
        {
            _characterHp = _characterMaxHp;
            return;
        }

        int missingHp = Mathf.Max(0, previousMaxHp - previousHp);
        _characterHp = Mathf.Clamp(_characterMaxHp - missingHp, 0, _characterMaxHp);
    }

    private async UniTask UseSkill(Monster targetMonster, MonsterParty monsterParty)
    {
        SetSingleTargetTransform(targetMonster);

        int currentDamage = _characterAtk * _skill.GetSkillDamage();

        if (_isSkillUsable == true)
        {
            ChangeState(CharacterState.SkillAttack);
            _skill.UseSkillAsync().Forget();
            await UniTask.Delay(GetSkillDuration());

            //[방어코드1 추가]
            if (this == null || this.gameObject == null || _isDead)
            {
                return;
            }
            //[방어코드1 끝]

            if (_skill.GetSkillType() == SkillType.SelfTarget)
            {
                this.TakeDamage(currentDamage).Forget();
                Debug.Log($"[힐스킬] 타겟{this.name}에게 {-currentDamage} 힐을 줍니다.");
            }
            else if (_skill.GetSkillType() == SkillType.MultiTarget || _skill.GetSkillType() == SkillType.MultiTarget_SelfSpawn)
            {
                //[방어코드2 추가]
                if (monsterParty != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var targetMonsterInParty = monsterParty.GetMonster(i);

                        //[방어코드3 추가]
                        if ((targetMonsterInParty != null) && (targetMonsterInParty._isDead == false))
                        {
                            targetMonsterInParty.TakeDamage(currentDamage).Forget();
                            Debug.Log($"[스킬공격] 타겟{targetMonsterInParty.name}에게 {currentDamage} 데미지를 줍니다.");
                        }
                    }
                }
            }
            else
            {
                //[방어코드 4 추가]
                if ((targetMonster != null) && (targetMonster._isDead == false))
                {
                    targetMonster.TakeDamage(currentDamage).Forget();
                    Debug.Log($"[스킬공격] 타겟{targetMonster.name}에게 {currentDamage} 데미지를 줍니다.");
                }
            }
        }
        CheckSkillUseable();
        InvokeCostChangedEvent();
    }

    public async UniTask AtkTarget(Monster targetMonster, MonsterParty monsterParty)
    {
        _isRunning = false;

        if (targetMonster._isDead == true) return;

        var characterType = _characterData.CharacterType;

        CheckSkillUseable();

        if (_isSkillUsable == false)
        {
            await UseNormalAttack(targetMonster);
            Debug.Log($"[일반공격] 타겟{targetMonster.name}에게 {_characterAtk} 데미지를 줍니다.");
            IncreaseCurrentSkillCost(1);
        }
        else
        {
            await UseSkill(targetMonster, monsterParty);
            UseSkillCost(_skill.GetRequiredSkillCost());
        }

        ChangeState(CharacterState.Idle);

        InvokeCostChangedEvent();
    }

    public void RunningCharacter()
    {
        if (_isRunning == true) return;
        _isRunning = true;
        ChangeState(CharacterState.Run);
    }

    private void SetSingleTargetTransform(Monster targetMonster)
    {
        if (_skill.GetSkillType() != SkillType.SingleTarget && _skill.GetSkillType() != SkillType.MultiTarget) return;
        var singleTargetTransform = targetMonster.gameObject.transform;
        _skill.SetSingleTargetTransform(singleTargetTransform);
    }

    private async UniTask UseNormalAttack(Monster targetMonster)
    {

        if (this == null || this.gameObject == null || _isDead)
        {
            return;
        }

        ChangeState(CharacterState.NormalAttack);

        var characterType = _characterData.CharacterType;

        await UniTask.Delay(GetNormalAttackMotionDuration(SetCharacterType(characterType)));

        if (targetMonster != null && targetMonster._isDead == false)
        {
            targetMonster.TakeDamage(_characterAtk).Forget();
        }

        await UniTask.Delay(200);
    }

    public void IncreaseCurrentSkillCost(int amount)
    {
        _currentSkillCost += amount;
        if (_currentSkillCost > _maxSkillCost)
        {
            _currentSkillCost = _maxSkillCost;
        }

    }

    public void UseSkillCost(int amount)
    {
        if (_currentSkillCost < amount)
        {
            Debug.Log("스킬 코스트가 부족합니다!");
        }
        _currentSkillCost -= amount;

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
                    return 800;
                }
            case NormalAttackType.Wizard:
                {
                    return 800;
                }
            case NormalAttackType.Monk:
                {
                    return 800;
                }
            default:
                {
                    return 800;
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

    public int GetCharacterLevel()
    {
        _characterLevel = (_characterCurrentExp / _needExpForLevelUp);
        return _characterLevel;
    }

    public void LevelUp()
    {
        CheckMaxLevelForRarity();

        if (_characterLevel >= _maxLevelForRarity) return;
        int expForLevelUp = _characterLevelManager.UseExpForLevelUp();
        _characterCurrentExp += expForLevelUp;
        _characterLevel = (_characterCurrentExp / _needExpForLevelUp);
        if (_characterLevel >= _maxLevelForRarity)
        {
            _characterLevel = _maxLevelForRarity;
        }
        InvokeLevelChangedEvent();
    }

    private void CheckMaxLevelForRarity()
    {
        switch (_characterData.Rarity)
        {
            case "C":
                {
                    _maxLevelForRarity = 5 + CheckCharacterUpgrade(4);
                }
                break;
            case "B":
                {
                    _maxLevelForRarity = 10 + CheckCharacterUpgrade(4);
                }
                break;
            case "A":
                {
                    _maxLevelForRarity = 15 + CheckCharacterUpgrade(5);
                }
                break;
            case "S":
                {
                    _maxLevelForRarity = 25 + CheckCharacterUpgrade(5);
                }
                break;
        }
    }

    private int CheckCharacterUpgrade(int increaseAmount)
    {
        return _characterEnhancement * increaseAmount;
    }

    public void UpgradeCharacter()
    {
        if (_characterEnhancement >= 5)
        {
            Debug.Log($"{_characterId} 캐릭터는 이미 최대 강화 상태입니다.");
            return;
        }
        _characterEnhancement += 1;
        Debug.Log($"{_characterId} 캐릭터가 강화되었습니다.");
    }

    private void IncreaseStatPerLevel()
    {
        _characterMaxHp += (_characterData.HpPerLevel * _characterLevel);
        _characterAtk += (_characterData.AtkPerLevel * _characterLevel);
        _characterDefense += (_characterData.DefPerLevel * _characterLevel);
    }

    // 테스트용 치트 함수 =======================================================

        private void Update()   // 테스트용으로만 업데이트 사용
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                TestGetMaxSkillCost();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                _characterLevelManager.AddExp(5000);
                Debug.Log($"경험치 5000증가, 현재 경험치: {_characterLevelManager.GetCurrentExp()}");
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                LevelUp();
                Debug.Log($"[{_characterId}]\n현재레벨: {_characterLevel}\n현재경험치: {_characterCurrentExp}\n현재강화단계: {_characterEnhancement}");
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                UpgradeCharacter();
            }
    }

    private void TestGetMaxSkillCost()
    {
        _currentSkillCost = _maxSkillCost;
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

    private void BindEquipmentChangedEvent()
    {
        if (_isEquipmentEventBound)
        {
            return;
        }

        if (NetworkManager.Instance == null || NetworkManager.Instance.EquipmentService == null)
        {
            return;
        }

        //NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged += OnCharacterEquipmentChanged;
        _isEquipmentEventBound = true;
    }

    private void UnbindEquipmentChangedEvent()
    {
        if (_isEquipmentEventBound == false)
        {
            return;
        }

        if (NetworkManager.Instance != null && NetworkManager.Instance.EquipmentService != null)
        {
            //NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged -= OnCharacterEquipmentChanged;
        }

        _isEquipmentEventBound = false;
    }

    private void OnCharacterEquipmentChanged(string characterUniqueId)
    {
        if (characterUniqueId != _characterUniqueId)
        {
            return;
        }

        SetStatData(false);
        InvokeStatChangedEvent();
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

    private void BindOnLevelChangedEvent(Action<int> levelChangedEventCallBack)
    {
        _onCharacterLevelChange += levelChangedEventCallBack;
    }

    private void InvokeLevelChangedEvent()
    {
        _onCharacterLevelChange?.Invoke(_characterLevel);
        IncreaseStatPerLevel();
    }

    // 캐릭터의 강화 단계와 레벨의 상승을 통하여 능력치의 변화를 동기화해주는 메서드
    public void RefreshStatFromSaveData()
    {
        if (_isDead == true)
        {
            return;
        }

        if (SaveManager.Instance != null && SaveManager.Instance.CharacterDict.TryGetValue(_characterUniqueId, out var saveData))
        {
            _characterEnhancement = saveData.Rank;
            _characterCurrentExp = (int)saveData.Exp;
            _characterLevel = 1 + (_characterCurrentExp / _needExpForLevelUp);
        }

        SetStatData(false);
        InvokeStatChangedEvent();
    }
}
