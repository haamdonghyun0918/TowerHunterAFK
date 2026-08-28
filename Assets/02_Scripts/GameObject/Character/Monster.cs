using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Monster : BattleCharacter
{
    [Header("데이터 관련")]
    private MonsterData _monsterData;
    private string _monsterId;
    private bool _isBoss;
    private Skill _skill;

    private void Awake()
    {
        this.gameObject.SetActive(true);

        if (_skill == null)
        {
            _skill = GetComponent<Skill>();
            if (_skill == null)
            {
                Debug.LogError($"[Monster] 스킬 컴포넌트를 가져오지 못했습니다.");
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
        }
        else
        {
            _characterAtk = ((_monsterData.BaseAtk) + stageNum);
            _characterAtkSpeed = _monsterData.BaseAtkSpeed;
            _characterHp = _monsterData.BaseHp + stageNum;
            _characterMaxHp = _monsterData.BaseHp + stageNum;
            _characterDefense = _monsterData.BaseDef + stageNum;
        }
            
    }

    public async UniTask AtkTarget(Character TargetCharacter)
    {
        if (TargetCharacter._isDead == true) return;

        ChangeState(CharacterState.NormalAttack);



        await TargetCharacter.TakeDamage(_characterAtk);

        await UniTask.Delay(800);

        Debug.Log($"타겟{TargetCharacter.name}에게 {_characterAtk} 데미지를 줍니다.");
        ChangeState(CharacterState.Idle);
    }

    public async UniTask UseProjectileSkill(Character targetCharacter)
    {
        if (_monsterData == null) return;
        if (_monsterData.IsBoss == false) return;

        float skillDuration = GetSkillDuration();

        ChangeState(CharacterState.SkillAttack);
        var skillData = _skill.GetSkillData();

        await UniTask.Delay(skillData.SkillDuration);

        await _skill.UseProjectileSkillAsync(this.gameObject.transform, targetCharacter, skillDuration);
        targetCharacter.TakeDamage(GetCurrentDamage()).Forget();


        await UniTask.Delay(1050);

        await _skill.UseProjectileSkillAsync(this.gameObject.transform, targetCharacter, skillDuration);
        targetCharacter.TakeDamage(GetCurrentDamage()).Forget();

        await UniTask.Delay(1800);
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