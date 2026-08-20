using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class HunterSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text _hunterName;
    [SerializeField] private UiButton _hunterSlotButton;
    [SerializeField] private Image _hunterIcon;
    [SerializeField] private Image _progressRing;

    private string _uniqueId;
    private Action<string> _onClickSlot;
    private Action<string> _onLongPressSlot;

    private CancellationTokenSource _cts;
    private CancellationTokenSource _longPressCts;

    private bool _isPointerDown = false;
    private bool _isLongPressTriggered = false;

    private bool _isClickCanceled = false;
    private const float TapThreshold = 0.2f;
    private const float LongPressDuration = 0.6f;

    public void SetUp(CharacterData data, string uniqueId, Action<string> onClick = null, Action<string> onLongPress = null)
    {
        _uniqueId = uniqueId;
        _onClickSlot = onClick;
        _onLongPressSlot = onLongPress;

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

        if (_progressRing != null)
        {
            _progressRing.fillAmount = 0f;
            _progressRing.gameObject.SetActive(false);
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
        if (_isLongPressTriggered || _isClickCanceled)
        {
            return;
        }

        _onClickSlot?.Invoke(_uniqueId);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _longPressCts?.Cancel();
        _longPressCts?.Dispose();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _isLongPressTriggered = false;
        _isClickCanceled = false;

        _longPressCts?.Cancel();
        _longPressCts?.Dispose();
        _longPressCts = new CancellationTokenSource();

        CheckLongPress(_longPressCts.Token).Forget();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        _longPressCts?.Cancel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerDown = false;
        _longPressCts?.Cancel();
    }

    private async UniTaskVoid CheckLongPress(CancellationToken token)
    {
        if (_progressRing == null)
        {
            Debug.Log("원형 이미지가 존재 하지 않습니다.");
            return;
        }

        float elapsedTime = 0f;
        _progressRing.fillAmount = 0f;
        _progressRing.gameObject.SetActive(false);

        try
        {
            while (elapsedTime < LongPressDuration)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= TapThreshold)
                {
                    _isClickCanceled = true;
                    _progressRing.gameObject.SetActive(true);

                    float fillRatio = (elapsedTime - TapThreshold) / (LongPressDuration - TapThreshold);
                    _progressRing.fillAmount = fillRatio;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }

            if (_isPointerDown)
            {
                _progressRing.fillAmount = 1f;
                _isLongPressTriggered = true;
                _onLongPressSlot?.Invoke(_uniqueId);
            }
        }

        catch
        {
        }

        finally
        {
            _progressRing.fillAmount = 0f;
            _progressRing.gameObject.SetActive(false);
        }
    }
}