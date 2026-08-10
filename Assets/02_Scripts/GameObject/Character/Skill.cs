using UnityEngine;
using UnityEngine.AddressableAssets;

public class Skill : MonoBehaviour
{
    private string _skillId;
    private string _skillPrefabPath;
    private string _skillType;
    private SkillData _skillData;
    private CharacterData _characterData;
    private Character _character;
    private Transform _targetTransform;

    private void OnEnable()
    {
        _skillData = GameDataManager.Instance.GetData<SkillData>(_skillId);
        SetPrefabPath();
    }

    public void SetSkillId(string skillId)
    {
        _skillId = skillId;
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
