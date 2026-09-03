using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Monster : BattleCharacter
{
    [Header("데이터 관련")]
    private MonsterData _monsterData;
    private string _monsterId;
    private bool _isBoss;
    private bool _isSkillUsable;
    private Skill _skill;

    private int _currentDamage;

    private int _currentSkillCost;

    private int _buffValue = 0;

    private event Action<int, int> _onSkillCostChange;

    private void Awake()
    {
        this.gameObject.SetActive(true);

        if (_skill == null)
        {
            _skill = GetComponent<Skill>();
            if (_skill != null)
            {
                Debug.LogWarning($"[Monster] 스킬 컴포넌트를 가져왔습니다. {this.name}");
            }
        }
    }

    private void OnEnable()
    {
        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log("[Monster] GameDataManager가 NULL입니다.");
        }
    }

    public void InitMonster(MonsterData monsterData, int stageNum)
    {
        _monsterData = monsterData;

        if (_monsterData == null)
        {
            Debug.LogError("[Monster]데이터가 Null입니다.");
            return;
        }

        _isDead = false;
        _isSkillUsable = false;

        _currentSkillCost = 0;

        _monsterId = _monsterData.Id;
        _isBoss = _monsterData.IsBoss;

        InitializeSkill();
        SetStatData(stageNum);
    }

    private void InitializeSkill()
    {
        if (_monsterData == null) return;
        if (_monsterData.IsBoss == false) return;
        string skillId = _monsterData.SkillId;

        if (_skill == null)
        {
            Debug.LogError($"[Monster] 스킬 데이터를 불러오지 못했습니다.");
            return;
        }

        _skill.InitializeSkill(skillId);
    }

    private void SetStatData(int stageNum)
    {
        if (_isBoss == true) 
        {
            _characterAtk = _monsterData.BaseAtk;
            _characterAtkSpeed = _monsterData.BaseAtkSpeed;
            _characterHp = _monsterData.BaseHp;
            _characterMaxHp = _monsterData.BaseHp;
            _characterDefense = _monsterData.BaseDef;
            _currentDamage = _characterAtk * _skill.GetSkillDamage();
        }
        else
        {
            _characterAtk = ((_monsterData.BaseAtk) + stageNum);
            _characterAtkSpeed = _monsterData.BaseAtkSpeed;
            _characterHp = _monsterData.BaseHp + stageNum;
            _characterMaxHp = _monsterData.BaseHp + stageNum;
            _characterDefense = _monsterData.BaseDef + stageNum;
            _currentDamage = _characterAtk;
        }


    }

    public async UniTask AtkTarget(Character targetCharacter, PlayerPartyControllerForBoss playerParty = null)
    {
        if (targetCharacter._isDead == true) return;

        if (_isSkillUsable == true && _isBoss == true)
        {
            if (_monsterData.SkillType == "Projectile")
            {
                await UseProjectileSkill(targetCharacter);
                UseSkillCost();
                return;
            }
            await UseSkill(targetCharacter, playerParty);
            UseSkillCost();
            return;
        }

        ChangeState(CharacterState.NormalAttack);

        await targetCharacter.TakeDamage(_characterAtk);

        await UniTask.Delay(800);

        Debug.Log($"타겟{targetCharacter.name}에게 {_characterAtk} 데미지를 줍니다.");
        ChangeState(CharacterState.Idle);

        if (_isBoss == true)
        {
            IncreaseSkillCost(1);
        }
        CheckSkillUseable();
    }

    private async UniTask UseSkill(Character targetCharacter, PlayerPartyControllerBase playerParty = null)
    {
        SetSingleTargetTransform(targetCharacter);

        if (_isSkillUsable == true)
        {
            ChangeState(CharacterState.SkillAttack);
            _skill.UseSkillAsync().Forget();
            await UniTask.Delay(GetSkillDuration());

            if (this == null || this.gameObject == null || _isDead)
            {
                return;
            }

            if (_skill.GetSkillType() == SkillType.SelfTarget)
            {
                _buffValue = 1000;
                _currentDamage += _buffValue;
                _characterDefense += 10;
                Debug.LogError($"[Monster] 버프스킬: 현재 데미지는 {_currentDamage}, 현재 방어력은 {_characterDefense}입니다.");
            }

            else if (_skill.GetSkillType() == SkillType.MultiTarget || _skill.GetSkillType() == SkillType.MultiTarget_SelfSpawn)
            {
                if (playerParty != null)
                {
                    for (int i = 0; i < playerParty.GetCurrentHunterCount(); i++)
                    {
                        var targetCharacterInParty = playerParty.GetHunter(i);

                        if ((targetCharacterInParty != null) && (targetCharacterInParty._isDead == false))
                        {
                            targetCharacterInParty.TakeDamage(_currentDamage).Forget();
                            Debug.Log($"[Monster] [스킬공격] 타겟{targetCharacterInParty.name}에게 {_currentDamage} 데미지를 줍니다.");
                        }
                    }
                }
            }
            else
            {
                if ((targetCharacter != null) && (targetCharacter._isDead == false))
                {
                    targetCharacter.TakeDamage(_currentDamage).Forget();
                    Debug.Log($"[Monster] [스킬공격] 타겟{targetCharacter.name}에게 {_currentDamage} 데미지를 줍니다.");
                }
            }
            ChangeState(CharacterState.Idle);
        }
        InvokeCostChangedEvent();
    }

    public async UniTask UseProjectileSkill(Character targetCharacter)
    {
        if (_monsterData == null) return;
        if (_monsterData.IsBoss == false) return;

        float skillDuration = GetSkillDuration();

        ChangeState(CharacterState.SkillAttack);
        var skillData = _skill.GetSkillData();

        await UniTask.Delay(skillData.SkillDuration);

        if (this == null || this.gameObject == null)
        {
            return;
        }

        await _skill.UseProjectileSkillAsync(this.gameObject.transform, targetCharacter, skillDuration);

        if (targetCharacter != null && targetCharacter.gameObject != null)
        {
            targetCharacter.TakeDamage(GetCurrentDamage()).Forget();
        }

        await UniTask.Delay(1050);

        if (this == null || this.gameObject == null)
        {
            return;
        }

        await _skill.UseProjectileSkillAsync(this.gameObject.transform, targetCharacter, skillDuration);
        targetCharacter.TakeDamage(GetCurrentDamage()).Forget();

        await UniTask.Delay(1800);
    }

    private void SetSingleTargetTransform(Character targetCharacter)
    {
        if (_skill.GetSkillType() != SkillType.SingleTarget && _skill.GetSkillType() != SkillType.MultiTarget) return;
        var singleTargetTransform = targetCharacter.gameObject.transform;
        _skill.SetSingleTargetTransform(singleTargetTransform);
    }

    private void CheckSkillUseable()
    {
        if (_isBoss == false) return;

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

    private void UseSkillCost()
    {
        int requiredCost = _skill.GetRequiredSkillCost();
        _currentSkillCost -= requiredCost;
        _isSkillUsable = false;
    }

    private void IncreaseSkillCost(int amount)
    {
        _currentSkillCost += amount;

        if (_currentSkillCost >= 2)
        {
            _currentSkillCost = 2;
        }
    }

    private void InvokeCostChangedEvent()
    {
        _onSkillCostChange?.Invoke(_currentSkillCost, 2);
    }

    private int GetCurrentDamage()
    {
        int currentDamage = _monsterData.BaseAtk + _skill.GetSkillDamage();
        return currentDamage;
    }

    public bool GetIsMonsterBoss()
    {
        return _isBoss;
    }

    private int GetSkillDuration()
    {
        string skillId = _monsterData.SkillId;
        var skillData = GameDataManager.Instance.GetData<SkillData>(skillId);
        int skillDuration = skillData.SkillDuration;
        return skillDuration;
    }
}