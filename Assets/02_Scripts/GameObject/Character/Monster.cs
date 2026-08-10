using System;
using UnityEngine;

public class Monster : BattleCharacter
{
    [Header("데이터 관련")]
    private MonsterData _monsterData;

    [Header("전투 관련")]
    [SerializeField] private GameObject TargetCharacter;
    private Character _targetCharacterComponent;


    private Animator _monsterAnimator;

    private Action<int, int> _onChangedHp;

    private void Awake()
    {
        this.gameObject.SetActive(true);
        _targetCharacterComponent = TargetCharacter.GetComponentInChildren<Character>();
    }

    private void OnEnable()
    {

        //[TODO] Hud 생성, 오브젝트매니저에 캐릭터 등록(소통후)
        if (GameDataManager.Instance == null)
        {
            Debug.Log("[Monster] GameDataManager가 NULL입니다.");
        }

        _monsterData = GameDataManager.Instance.GetData<MonsterData>("monster_Test_01");    // [TODO] 하드코딩을 하지않고 (ID)데이터를 받아와야함
        SetStatData();
    }

    private void SetStatData()
    {
        _characterAtk = _monsterData.BaseAtk;
        _characterAtkSpeed = _monsterData.BaseAtkSpeed;
        _characterHp = _monsterData.BaseHp;
        _characterMaxHp = _monsterData.BaseHp;
    }

    public void AtkTarget(Character TargetCharacter)
    {
        if (TargetCharacter._isDead == true) return;

        Debug.Log($"타겟{TargetCharacter.name}에게 {_characterAtk} 데미지를 줍니다.");
        //[TODO] 평타공격 모션
        TargetCharacter.TakeDamage(_characterAtk);
    }
}