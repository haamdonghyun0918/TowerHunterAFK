using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private Image Image_EquipmentIcon;
    [SerializeField] private TMP_Text Text_EnhanceLevel;
    [SerializeField] private UiButton Button_Equipment;
    [SerializeField] private Image _progressRing;

    private string _uniqueId;
    private Action<string> _onClickEquipment;
    private Action<string> _onLongPressEquipment;

    private CancellationTokenSource _iconCancellationTokenSource;
    private CancellationTokenSource _longPressCts;

    private bool _isPointerDown = false;
    private bool _isLongPressTriggered = false;
    private bool _isClickCanceled = false;
    private const float TapThreshold = 0.2f;
    private const float LongPressDuration = 0.6f;


    public void SetUp(EquipmentSlotViewModel viewModel, Action<string> onClickEquipment, Action<string> onLongPressEquipment = null)
    {
        if (viewModel == null)
        {
            Clear();
            return;
        }

        _uniqueId = viewModel.UniqueId;
        _onClickEquipment = onClickEquipment;
        _onLongPressEquipment = onLongPressEquipment;

        _longPressCts?.Cancel();
        _longPressCts?.Dispose();
        _longPressCts = new CancellationTokenSource();

        if (_progressRing != null)
        {
            _progressRing.fillAmount = 0f;
            _progressRing.gameObject.SetActive(false);
        }

        UpdateEnhanceLevel(viewModel.EnhanceLevel);
        BindButton();
        RequestLoadIcon(viewModel.IconAddress);
    }

    private void UpdateEnhanceLevel(int enhanceLevel)
    {
        if (Text_EnhanceLevel == null)
        {
            return;
        }

        Text_EnhanceLevel.text = $"+{enhanceLevel}"; 
    }

    private void BindButton()
    {
        if (Button_Equipment == null)
        {
            return;
        }

        Button_Equipment.UnBindOnClickButtonEvent(OnClickEquipment);
        Button_Equipment.BindOnClickButtonEvent(OnClickEquipment);
    }

    private void OnClickEquipment()
    {
        if (_isLongPressTriggered || _isClickCanceled)
        {
            return;
        }

        if (_onClickEquipment == null)
        {
            return;
        }

        _onClickEquipment.Invoke(_uniqueId);
    }

    private void RequestLoadIcon(string iconAddress)
    {
        CancelIconLoad();
        HideIcon();

        if (string.IsNullOrEmpty(iconAddress))
        {
            return;
        }

        _iconCancellationTokenSource = new CancellationTokenSource();
        LoadIconAsync(iconAddress, _iconCancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid LoadIconAsync(string iconAddress, CancellationToken cancellationToken)
    {
        if (ResourceManager.Instance == null)
        {
            return;
        }

        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(iconAddress);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (loadedSprite == null || Image_EquipmentIcon == null)
        {
            return;
        }

        Image_EquipmentIcon.sprite = loadedSprite;
        Image_EquipmentIcon.gameObject.SetActive(true);
    }

    private void HideIcon()
    {
        if (Image_EquipmentIcon == null)
        {
            return;
        }

        Image_EquipmentIcon.sprite = null;
        Image_EquipmentIcon.gameObject.SetActive(false);
    }

    private void Clear()
    {
        _uniqueId = "";
        _onClickEquipment = null;
        _onLongPressEquipment = null;

        CancelIconLoad();
        HideIcon();

        if (Text_EnhanceLevel != null)
        {
            Text_EnhanceLevel.text = "";
        }

        if (_progressRing != null)
        {
            _progressRing.fillAmount = 0f;
            _progressRing.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        CancelIconLoad();
        _longPressCts?.Cancel();
    }

    private void OnDestroy()
    {
        CancelIconLoad();

        _longPressCts?.Cancel();
        _longPressCts?.Dispose();

        if (Button_Equipment != null)
        {
            Button_Equipment.UnBindOnClickButtonEvent(OnClickEquipment);
        }
    }

    private void CancelIconLoad()
    {
        if (_iconCancellationTokenSource == null)
        {
            return;
        }

        _iconCancellationTokenSource.Cancel();
        _iconCancellationTokenSource.Dispose();
        _iconCancellationTokenSource = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _isLongPressTriggered = false;
        _isClickCanceled = false;

        if (_onLongPressEquipment == null)
        {
            return;
        }

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
            Debug.LogWarning("[EquipmentSlotView] ProgressRing 이미지가 존재하지 않습니다.");
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
                _onLongPressEquipment?.Invoke(_uniqueId);
            }
        }

        catch 
        {
        }

        finally
        {
            if (_progressRing != null)
            {
                _progressRing.fillAmount = 0f;
                _progressRing.gameObject.SetActive(false);
            }
        }
    }
}