using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
public class HitFlashEffect : MonoBehaviour
{
    [Header("피격 반짝임")]
    [SerializeField] private Renderer[] _targetRenderers;
    [SerializeField] private Material _flashMaterial;
    [SerializeField] private float _flashDuration = 0.08f;

    private Material[][] _originalMaterials;
    private Material[][] _flashMaterials;
    private CancellationTokenSource _flashCancellationTokenSource;

    private void Awake()
    {
        if (_targetRenderers == null || _targetRenderers.Length == 0)
        {
            _targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        CacheMaterials();
    }

    private void OnDisable()
    {
        StopHitFlash();
        RestoreOriginalMaterials();
    }

    private void OnDestroy()
    {
        StopHitFlash();
    }

    public void PlayHitFlash()
    {
        if (_flashMaterial == null)
        {
            Debug.LogWarning($"[HitFlashEffect] Flash Material이 연결되지 않았습니다. {name}");
            return;
        }

        if (_targetRenderers == null || _targetRenderers.Length == 0)
        {
            Debug.LogWarning($"[HitFlashEffect] Renderer를 찾지 못했습니다. {name}");
            return;
        }

        StopHitFlash();

        _flashCancellationTokenSource = new CancellationTokenSource();

        ApplyFlashMaterials();
        PlayHitFlashAsync(_flashCancellationTokenSource.Token).Forget();
    }

    private async UniTask PlayHitFlashAsync(CancellationToken cancellationToken)
    {
        try
        {
            int durationMilliseconds = Mathf.CeilToInt(_flashDuration * 1000f);

            await UniTask.Delay(durationMilliseconds, cancellationToken: cancellationToken);

            RestoreOriginalMaterials();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CacheMaterials()
    {
        _originalMaterials = new Material[_targetRenderers.Length][];
        _flashMaterials = new Material[_targetRenderers.Length][];

        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            Renderer targetRenderer = _targetRenderers[i];

            if (targetRenderer == null)
            {
                continue;
            }

            _originalMaterials[i] = targetRenderer.sharedMaterials;
            _flashMaterials[i] = new Material[_originalMaterials[i].Length];

            for (int j = 0; j < _flashMaterials[i].Length; j++)
            {
                _flashMaterials[i][j] = _flashMaterial;
            }
        }
    }

    private void ApplyFlashMaterials()
    {
        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            Renderer targetRenderer = _targetRenderers[i];

            if (targetRenderer == null || _flashMaterials[i] == null)
            {
                continue;
            }

            targetRenderer.sharedMaterials = _flashMaterials[i];
        }
    }

    private void RestoreOriginalMaterials()
    {
        if (_targetRenderers == null || _originalMaterials == null)
        {
            return;
        }

        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            Renderer targetRenderer = _targetRenderers[i];

            if (targetRenderer == null || _originalMaterials[i] == null)
            {
                continue;
            }

            targetRenderer.sharedMaterials = _originalMaterials[i];
        }
    }

    private void StopHitFlash()
    {
        if (_flashCancellationTokenSource == null)
        {
            return;
        }

        _flashCancellationTokenSource.Cancel();
        _flashCancellationTokenSource.Dispose();
        _flashCancellationTokenSource = null;
    }
}