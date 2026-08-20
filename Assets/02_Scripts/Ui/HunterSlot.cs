using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class HunterSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text _hunterName;
    [SerializeField] private UiButton _hunterSlotButton;
    [SerializeField] private Image _hunterIcon;

    private string _uniqueId;
    private Action<string> _onClickSlot;

    private CancellationTokenSource _cts;

    public void SetUp(CharacterData data, string uniqueId, Action<string> onClick = null)
    {
        _uniqueId = uniqueId;
        _onClickSlot = onClick;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        if (data != null)
        {
            _hunterName.text = data.Name;

            if (string.IsNullOrEmpty(data.IconPath) == false)
            {
                _hunterIcon.sprite = null;
                _hunterIcon.gameObject.SetActive(false);
                LoadIconAsync(data.IconPath, _cts.Token).Forget();
            }

            else
            {
                _hunterIcon.sprite = null;
                _hunterIcon.gameObject.SetActive(false);
            }
        }

        else
        {
            _hunterName.text = "비어있음";
            if (_hunterIcon != null)
            {
                _hunterIcon.sprite = null;
                _hunterIcon.gameObject.SetActive(false);
            }
        }

        if (_hunterSlotButton != null)
        {
            _hunterSlotButton.UnBindOnClickButtonEvent(OnClickSlot);
            _hunterSlotButton.BindOnClickButtonEvent(OnClickSlot);
        }
    }

    private async UniTaskVoid LoadIconAsync(string iconPath, CancellationToken token)
    {
        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(iconPath);
       
        if (token.IsCancellationRequested)
        {
            return;
        }

        if (loadedSprite != null)
        {
            _hunterIcon.sprite = loadedSprite;
            _hunterIcon.gameObject.SetActive(true);
        }

        else
        {
            Debug.LogWarning($"[HunterSlot] 아이콘 로드 실패: {iconPath}");
        }
    }

    private void OnClickSlot()
    {
        _onClickSlot?.Invoke(_uniqueId);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

}