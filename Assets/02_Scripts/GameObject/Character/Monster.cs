using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Monster : BattleCharacter
{
    [Header("데이터 관련")]
    private MonsterData _monsterData;
    private string _monsterId;
    private bool _isBoss;

    private void Awake()
    {
        this.gameObject.SetActive(true);
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

        SetStatData(stageNum);
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

        await UniTask.Delay(1000);

        await TargetCharacter.TakeDamage(_characterAtk);

        Debug.Log($"타겟{TargetCharacter.name}에게 {_characterAtk} 데미지를 줍니다.");
        ChangeState(CharacterState.Idle);
    }

    public bool GetIsMonsterBoss()
    {
        return _isBoss;
    }
}