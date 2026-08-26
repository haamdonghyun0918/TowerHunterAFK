using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotView : MonoBehaviour
{
    [SerializeField] private Image Image_EquipmentIcon;
    [SerializeField] private TMP_Text Text_EnhanceLevel;
    [SerializeField] private UiButton Button_Equipment;
    [SerializeField] private GameObject _hunterCannes;

    private string _uniqueId;
    private Action<string> _onClickEquipment;
    private CancellationTokenSource _iconCancellationTokenSource;

    public void SetUp(EquipmentSlotViewModel viewModel, Action<string> onClickEquipment)
    {
        if (viewModel == null)
        {
            Clear();
            return;
        }

        _uniqueId = viewModel.UniqueId;
        _onClickEquipment = onClickEquipment;

        UpdateEnhanceLevel(viewModel.EnhanceLevel);
        BindButton();
        RequestLoadIcon(viewModel.IconAddress);

        BindEquipmentEvent();
        UpdateEquippedStatus();
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

        CancelIconLoad();
        HideIcon();

        UnbindEquipmentEvent();

        if (Text_EnhanceLevel != null)
        {
            Text_EnhanceLevel.text = "";
        }

        if (_hunterCannes != null)
        {
            _hunterCannes.SetActive(false);
        }
    }

    private void BindEquipmentEvent()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[EquipmentSlotView] NetworkManager.Instance가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[EquipmentSlotView] EquipmentService가 없습니다.");
            return;
        }

        NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged -= OnCharacterEquipmentChanged;
        NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged += OnCharacterEquipmentChanged;
    }

    private void UnbindEquipmentEvent()
    {
        if (NetworkManager.Instance != null)
        {
            if (NetworkManager.Instance.EquipmentService != null)
            {
                NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged -= OnCharacterEquipmentChanged;
            }
        }
    }

    private void OnCharacterEquipmentChanged(string characterUniqueId)
    {
        UpdateEquippedStatus();
    }

    private void UpdateEquippedStatus()
    {
        if (_hunterCannes == null)
        {
            Debug.LogError("[EquipmentSlotView] Obj_EquippedMark가 연결되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(_uniqueId) == true)
        {
            _hunterCannes.SetActive(false);
            return;
        }

        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[EquipmentSlotView] NetworkManager.Instance가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[EquipmentSlotView] EquipmentService가 없습니다.");
            return;
        }

        bool isEquipped = NetworkManager.Instance.EquipmentService.IsEquipmentEquipped(_uniqueId);
        _hunterCannes.SetActive(isEquipped);
    }

    private void OnDisable()
    {
        CancelIconLoad();
    }

    private void OnDestroy()
    {
        CancelIconLoad();

        UnbindEquipmentEvent();

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
}