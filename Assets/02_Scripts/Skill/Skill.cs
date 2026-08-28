using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum SkillType
{
    None,
    SingleTarget,
    MultiTarget,
    SelfTarget,
    SingleTarget_SelfSpawn,
    MultiTarget_SelfSpawn
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
    private MonsterParty _monsterParty;

    private CancellationTokenSource _actionCancellationTokenSource = new CancellationTokenSource();

    private List<GameObject> _spawnedEffectInstances = new List<GameObject>();

    private void OnEnable()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged += OnStageChanged;
        }
    }

    private void OnDisable()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged -= OnStageChanged;
        }

        CancelAllTasks();
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
        SetTargetTransform();
    }

    private void CancelAllTasks()
    {
        _actionCancellationTokenSource?.Cancel();
        _actionCancellationTokenSource?.Dispose();
        _actionCancellationTokenSource = new CancellationTokenSource();
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

    public SkillData GetSkillData()
    {
        return _skillData;
    }

    public async UniTaskVoid UseSkillAsync()
    {
        if (_skillData == null)
        {
            Debug.LogError($"[Skill] 스킬 데이터가 초기화 되지 않았습니다.");
            return;
        }

        try
        {
            int motionDuration = _skillData.MotionDuration;

            await UniTask.Delay(motionDuration, cancellationToken: _actionCancellationTokenSource.Token);

            GameObject instance = await InstantiateAsync(_skillPrefabPath, _targetTransform);

            if (instance != null)
            {
                _spawnedEffectInstances.Add(instance);
            }
        }

        catch (System.OperationCanceledException)
        {

        }
        
    }

    public async UniTask UseProjectileSkillAsync(Transform parentTransform, Character targetCharacter, float duration)
    {
        if (_skillData == null)
        {
            Debug.LogError($"[Skill] 스킬 데이터가 초기화 되지 않았습니다.");
            return;
        }

        try
        {
            InstantiateAndShootSkill(_skillPrefabPath, parentTransform, duration).Forget();
        }

        catch (System.OperationCanceledException)
        {

        }

    }

    private async UniTask InstantiateAndShootSkill(string skillPrefabPath, Transform parentTransform, float duration)
    {
        GameObject instance = await InstantiateAsync(_skillPrefabPath, parentTransform);

        if (instance != null)
        {
            _spawnedEffectInstances.Add(instance);
        }
    }

    private async UniTask<GameObject> InstantiateAsync(string prefabPath, Transform parentTransform = null)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefabPath, parentTransform);

        try
        {
            GameObject instance = await handle.ToUniTask(cancellationToken: _actionCancellationTokenSource.Token);

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

    //private async UniTask<GameObject> InstantiateProjectileAsync(string prefabPath, Transform parentTransform = null, Transform targetTransform = null)
    //{
    //    AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefabPath, parentTransform);

    //    try
    //    {
    //        GameObject instance = await handle.ToUniTask(cancellationToken: _actionCancellationTokenSource.Token);

    //        return instance;
    //    }

    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"[Skill] 스킬 프리팹 생성 실패: {prefabPath} / 에러: {e.Message}");

    //        if (handle.IsValid())
    //        {
    //            Addressables.Release(handle);
    //        }

    //        return null;
    //    }
    //}
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

    private Transform GetTargetTransform(SkillType skillType)
    {
        Transform targetTransform = null;
        if (skillType == SkillType.MultiTarget || skillType == SkillType.SingleTarget)
        {
            targetTransform = _targetTransform;
        }

        else if (skillType == SkillType.SelfTarget || skillType == SkillType.SingleTarget_SelfSpawn || skillType == SkillType.MultiTarget_SelfSpawn)
        {
            targetTransform = this.gameObject.transform;
        }

        return targetTransform;
    }

    public void SetSingleTargetTransform(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }

    private Transform SetTargetTransform()
    {
        if (_skillData == null) return null;

        var skillType = GetSkillType();
        switch (skillType)
        {
            case SkillType.None:
                {
                    _targetTransform = null;
                }
                break;
            case SkillType.SingleTarget:
                {
                    _targetTransform = GetTargetTransform(SkillType.SingleTarget);
                }
                break;
            case SkillType.SelfTarget:
                {
                    _targetTransform = GetTargetTransform(SkillType.SelfTarget);
                }
                break;
            case SkillType.MultiTarget:
                {
                    _targetTransform = GetTargetTransform(SkillType.MultiTarget);
                }
                break;
            case SkillType.SingleTarget_SelfSpawn:
                {
                    _targetTransform = GetTargetTransform(SkillType.SingleTarget_SelfSpawn);
                }
                break;
            case SkillType.MultiTarget_SelfSpawn:
                {
                    _targetTransform = GetTargetTransform(SkillType.MultiTarget_SelfSpawn);
                }
                break;
        }
        return _targetTransform;
    }
    private void OnStageChanged(int newStage)
    {
        ClearAllSkillEffect();
    }

    public void ClearAllSkillEffect()
    {
        CancelAllTasks();

        foreach(var effect in _spawnedEffectInstances)
        {
            if (effect != null)
            {
                Addressables.ReleaseInstance(effect);
            }
        }
        _spawnedEffectInstances.Clear();
    }


    // 테스트용 함수 ==================================================

    
}
