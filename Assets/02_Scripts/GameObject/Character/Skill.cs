using UnityEngine;
using UnityEngine.AddressableAssets;

public class Skill : MonoBehaviour
{
    private string _skillId;
    private string _skillPrefabPath;
    private int _skillDamage;
    private int _RequiredSkillCost;
    private SkillData _skillData;
    private Transform _targetTransform;

    private void OnEnable()
    {
        _skillData = GameDataManager.Instance.GetData<SkillData>(_skillId);
        SetPrefabPath();
        SetRequiredSkillCost();
        SetSkillDamage();
    }

    public void SetSkillId(string skillId)
    {
        _skillId = skillId;
    }

    private void SetRequiredSkillCost()
    {
        _RequiredSkillCost = _skillData.RequiredCost;
    }

    public int GetRequiredSkillCost()
    {
        return _RequiredSkillCost;
    }

    public void UseSkill()
    {
        if (_skillData == null) return;
        Addressables.InstantiateAsync(_skillPrefabPath, _targetTransform);
    }

    private void SetPrefabPath()
    {
        if (_skillData == null) return;
        _skillPrefabPath = _skillData.PrefabPath;
    }

    private void SetSkillDamage()
    {
        if (_skillData == null) return;
        _skillDamage = _skillData.SkillDamage;
    }

    public int GetSkillDamage()
    {
        return _skillDamage;
    }

    public string GetSkillType()
    {
        if (_skillData == null) return null;
        return _skillData.SkillType;
    }

    public Transform SetTargetTransform(Transform targetTransform)
    {
        _targetTransform = targetTransform;
        return _targetTransform;
    }
}
