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

    private CancellationTokenSource _damageTextCancellationTokenSource;

    private void OnDisable()
    {
        StopDamageText();
        HideDamageText();
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
        Root_DamageText.SetActive(true);

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
