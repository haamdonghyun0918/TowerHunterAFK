using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum SkillType
{
    None,
    SingleTarget,
    MultiTarget,
    SelfTarget,
    TeamTarget
}

public class Skill : MonoBehaviour
{
    private string _skillId;
    private string _skillPrefabPath;
    private string _skillType;
    private int _skillDamage;
    private int _requiredSkillCost;
    private SkillData _skillData;
    private Transform _targetTransform;

    private void OnEnable()
    {

    }

    public void InitializeSkill(string skillId)
    {
        SetSkillId(skillId);
        _skillData = GameDataManager.Instance.GetData<SkillData>(_skillId);
        if (_skillData == null)
        {
            Debug.LogError($"[Skill] 스킬 데이터를 불러오지 못했습니다.");
            return;
        }
        SetBaseSkillDamage();
        SetPrefabPath();
        SetRequiredSkillCost();
        SetSkillType();
        TestSetTargetTransform();
    }

    private void SetSkillId(string skillId)
    {
        _skillId = skillId;
    }

    private void SetRequiredSkillCost()
    {
        if (_skillData == null) return;
        _requiredSkillCost = _skillData.RequiredCost;
    }

    public int GetRequiredSkillCost()
    {
        if (_skillData == null)
        {
            Debug.LogError($"[Skill] 스킬 데이터를 불러오지 못했습니다.");
            return 0;
        }
        return _requiredSkillCost;
    }

    public async UniTaskVoid UseSkillAsync()
    {
        if (_skillData == null)
        {
            Debug.LogError($"[Skill] 스킬 데이터가 초기화 되지 않았습니다.");
            return;
        }

        GameObject instance = await InstantiateAsync(_skillPrefabPath, _targetTransform);

    }

    private async UniTask<GameObject> InstantiateAsync(string prefabPath, Transform parentTransform = null)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefabPath, parentTransform);

        try
        {
            GameObject instance = await handle.ToUniTask();

            return instance;
        }

        catch(System.Exception e)
        {
            Debug.LogError($"[Skill] 스킬 프리팹 생성 실패: {prefabPath} / 에러: {e.Message}");

            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return null;
        }
    }

    private void SetPrefabPath()
    {
        if (_skillData == null) return;
        _skillPrefabPath = _skillData.PrefabPath;
    }

    private void SetBaseSkillDamage()
    {
        if (_skillData == null) return;
        _skillDamage = _skillData.SkillDamage;
    }

    public int GetSkillDamage()
    {
        return _skillDamage;
    }

    private SkillType SetSkillType()
    {
        if (_skillData == null) return SkillType.None;
        _skillType = _skillData.SkillType;

        try
        {
            return (SkillType)System.Enum.Parse(typeof(SkillType), _skillType, true);
        }

        catch
        {
            Debug.LogError($"[Skill] {_skillType}스킬타입 변환 실패");
            return SkillType.None;
        }
    }

    public SkillType GetSkillType()
    {
        if (_skillData == null) return SkillType.None;

        try
        {
            return (SkillType)System.Enum.Parse(typeof(SkillType), _skillType, true);
        }

        catch
        {
            Debug.LogError($"[Skill] {_skillType}스킬타입 변환 실패");
            return SkillType.None;
        }
    }

    public Transform SetTargetTransform(Transform targetTransform)
    {
        _targetTransform = targetTransform;
        return _targetTransform;
    }


    // 테스트용 함수 ==================================================

    private Transform TestSetTargetTransform()
    {
        if (_skillData == null) return null;

        var skillType = GetSkillType();
        switch (skillType)
        {
            case SkillType.SelfTarget:
                {
                    _targetTransform = this.gameObject.transform;
                    return _targetTransform;
                }
            case SkillType.SingleTarget:
                {
                    _targetTransform = this.gameObject.transform;
                    return _targetTransform;
                }
            default:
                {
                    return null;
                }
        }
    }
}
