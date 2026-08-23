using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentEnhanceUI : UiBase
{
    [SerializeField] private TMP_Text Text_EquipmentName;
    [SerializeField] private TMP_Text Text_TotalStat;
    [SerializeField] private TMP_Text Text_EnhanceCost;
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private Button Button_EnhanceBtn;
    [SerializeField] private Button Button_CloseAll;

    private EquipmentEnhanceViewModel _viewModel;

    private void OnEnable()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[EquipmentEnhanceUI] NetworkManager가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[EquipmentEnhanceUI] EquipmentService가 없습니다.");
            return;
        }

        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentEnhanceViewModel();

        if (_viewModel == null)
        {
            Debug.LogError("[EquipmentEnhanceUI] EquipmentEnhanceViewModel을 가져오지 못했습니다.");
            return;
        }

        BindViewModel();
        BindButtons();
        UpdateEnhanceUI();
    }

    private void OnDisable()
    {
        UnbindViewModel();
        UnbindButtons();
        _viewModel = null;
    }

    private void BindViewModel()
    {
        _viewModel.PropertyChanged -= OnPropertyChanged;
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void UnbindViewModel()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged;
        }
    }

    private void BindButtons()
    {
        Button_EnhanceBtn.onClick.RemoveListener(OnClickEnhanceButton);
        Button_EnhanceBtn.onClick.AddListener(OnClickEnhanceButton);

        Button_CloseAll.onClick.RemoveListener(OnClickCloseButton);
        Button_CloseAll.onClick.AddListener(OnClickCloseButton);
    }

    private void UnbindButtons()
    {
        Button_EnhanceBtn.onClick.RemoveListener(OnClickEnhanceButton);
        Button_CloseAll.onClick.RemoveListener(OnClickCloseButton);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        UpdateEnhanceUI();
    }

    private void UpdateEnhanceUI()
    {
        if (_viewModel == null || _viewModel.HasTarget == false)
        {
            ClearEnhanceUI();
            return;
        }

        Text_EquipmentName.text = $"{_viewModel.ItemName} +{_viewModel.EnhanceLevel}";
        Text_TotalStat.text = $"{_viewModel.TotalAtkText}\n{_viewModel.TotalDefText}";
        Text_EnhanceCost.text = $"강화 비용\n{_viewModel.CostText}";
        Button_EnhanceBtn.interactable = true;

        LoadIconAsync().Forget();
    }

    private void ClearEnhanceUI()
    {
        Text_EquipmentName.text = "";
        Text_TotalStat.text = "";
        Text_EnhanceCost.text = "";
        Image_ItemIcon.sprite = null;
        Image_ItemIcon.gameObject.SetActive(false);
        Button_EnhanceBtn.interactable = false;
    }

    private async UniTaskVoid LoadIconAsync()
    {
        string iconAddress = _viewModel.ItemIconAddress;

        if (string.IsNullOrEmpty(iconAddress))
        {
            Image_ItemIcon.sprite = null;
            Image_ItemIcon.gameObject.SetActive(false);
            return;
        }

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("[EquipmentEnhanceUI] ResourceManager가 없습니다.");
            Image_ItemIcon.sprite = null;
            Image_ItemIcon.gameObject.SetActive(false);
            return;
        }

        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(iconAddress);

        if (isActiveAndEnabled == false || _viewModel == null)
        {
            return;
        }

        if (_viewModel.ItemIconAddress != iconAddress)
        {
            return;
        }

        if (loadedSprite == null)
        {
            Debug.LogWarning($"[EquipmentEnhanceUI] 아이콘 로드에 실패했습니다: {iconAddress}");
            Image_ItemIcon.sprite = null;
            Image_ItemIcon.gameObject.SetActive(false);
            return;
        }

        Image_ItemIcon.sprite = loadedSprite;
        Image_ItemIcon.gameObject.SetActive(true);
    }

    private void OnClickEnhanceButton()
    {
        if (_viewModel == null || _viewModel.HasTarget == false)
        {
            return;
        }

        bool isEnhanced = _viewModel.RequestEnhance();

        if (isEnhanced == false)
        {
            Debug.LogWarning("[EquipmentEnhanceUI] 장비 강화에 실패했습니다.");
        }
    }

    private void OnClickCloseButton()
    {
        UiManager.Instance.CloseUi<EquipmentEnhanceUI>();
    }
}