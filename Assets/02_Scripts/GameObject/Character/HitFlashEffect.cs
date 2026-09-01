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

    }

    public void TakeDamageFlash()
    {
        if(_flashMaterial == null)
        {
            Debug.LogWarning($"[HitFlashEffect] Flash Material이 연결되지 않았습니다. {name}");
            return;
        }

        if(_targetRenderers == null || _targetRenderers.Lengt == 0)
        {
            Debug.LogWarning($"[HitFlashEffect] Renderer를 찾지 못했습니다. {name}");
            return;
        }

        _flashCancellationTokenSource = new CancellationTokenSource();

        
    }


}
