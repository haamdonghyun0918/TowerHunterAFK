using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class DamageTextEffect : MonoBehaviour
{
    [Header("피격 데미지 텍스트")]
    [SerializeField] private GameObject Root_DamageText;
    [SerializeField] private TMP_Text Text_Damage;
    [SerializeField] private float _displayDuration = 1f;
    
    private Transform _targetCameraTransform;

    private CancellationTokenSource _damageTextCancellationTokenSource;

    private void OnDisable()
    {
        StopDamageText();
        HideDamageText();
    }

    private void LateUpdate()
    {
        if (Root_DamageText == null || Root_DamageText.activeInHierarchy == false)
        {
            return;
        }

        FaceCamera();
    }

    private void OnDestroy()
    {
        StopDamageText();
    }

    public void ShowDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        if (Root_DamageText == null || Text_Damage == null)
        {
            Debug.LogWarning($"[DamageTextEffect] 데미지 텍스트 참조가 연결되지 않았습니다. {name}");
            return;
        }

        StopDamageText();

        Text_Damage.text = damage.ToString();
        SetTargetCameraFromObjectManager();
        Root_DamageText.SetActive(true);

        FaceCamera();

        _damageTextCancellationTokenSource = new CancellationTokenSource();
        HideDamageTextAsync(_damageTextCancellationTokenSource.Token).Forget();
    }

    private async UniTask HideDamageTextAsync(CancellationToken cancellationToken)
    {
        try
        {
            int durationMilliseconds = Mathf.CeilToInt(_displayDuration * 1000f);

            await UniTask.Delay(durationMilliseconds, cancellationToken: cancellationToken);

            HideDamageText();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void SetTargetCameraFromObjectManager()
    {
        _targetCameraTransform = null;

        if (ObjectManager.Instance == null)
        {
            return;
        }

        PlayerPartyCamera playerPartyCamera =
            ObjectManager.Instance.GetCurrentPlayerPartyCamera();

        if (playerPartyCamera == null)
        {
            Debug.LogWarning(
                "[DamageTextEffect] ObjectManager에 현재 PlayerPartyCamera가 없습니다.");

            return;
        }

        _targetCameraTransform = playerPartyCamera.transform;
    }

    private void FaceCamera()
    {
        if (_targetCameraTransform == null)
        {
            return;
        }

        Root_DamageText.transform.rotation =
            _targetCameraTransform.rotation;
    }

    private void HideDamageText()
    {
        if (Root_DamageText != null)
        {
            Root_DamageText.SetActive(false);
        }
    }

    private void StopDamageText()
    {
        if (_damageTextCancellationTokenSource == null)
        {
            return;
        }

        _damageTextCancellationTokenSource.Cancel();
        _damageTextCancellationTokenSource.Dispose();
        _damageTextCancellationTokenSource = null;
    }
}
