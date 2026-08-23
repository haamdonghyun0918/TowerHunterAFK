using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDetailUI : UiBase
{
    [SerializeField] private TMP_Text Text_EquipmentName;
    [SerializeField] private TMP_Text Text_TotalStat;
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private Button Button_Enhance;
    [SerializeField] private Button Button_Disassemble;
    [SerializeField] private Button Button_CloseAll;

    private EquipmentDetailViewModel _viewModel;

    private void OnEnable()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[EquipmentDetailUI] NetworkManager가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[EquipmentDetailUI] EquipmentService가 없습니다.");
            return;
        }

        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentDetailViewModel();

        if (_viewModel == null)
        {
            Debug.LogError("[EquipmentDetailUI] EquipmentDetailViewModel을 가져오지 못했습니다.");
            return;
        }

        BindViewModel();
        BindButtons();
        UpdateDetailUI();
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
        Button_Enhance.onClick.RemoveListener(OnClickEnhanceButton);
        Button_Enhance.onClick.AddListener(OnClickEnhanceButton);

        Button_Disassemble.onClick.RemoveListener(OnClickDisassembleButton);
        Button_Disassemble.onClick.AddListener(OnClickDisassembleButton);

        Button_CloseAll.onClick.RemoveListener(OnClickCloseButton);
        Button_CloseAll.onClick.AddListener(OnClickCloseButton);
    }

    private void UnbindButtons()
    {
        Button_Enhance.onClick.RemoveListener(OnClickEnhanceButton);
        Button_Disassemble.onClick.RemoveListener(OnClickDisassembleButton);
        Button_CloseAll.onClick.RemoveListener(OnClickCloseButton);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        UpdateDetailUI();
    }

    private void UpdateDetailUI()
    {
        if (_viewModel == null || _viewModel.HasTarget == false)
        {
            ClearDetailUI();
            return;
        }

        Text_EquipmentName.text = _viewModel.ItemName;
        Text_TotalStat.text = _viewModel.TotalStatText;

        LoadIconAsync().Forget();
    }

    private void ClearDetailUI()
    {
        Text_EquipmentName.text = "";
        Text_TotalStat.text = "";
        Image_ItemIcon.sprite = null;
        Image_ItemIcon.gameObject.SetActive(false);
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
            Debug.LogError("[EquipmentDetailUI] ResourceManager가 없습니다.");
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
            Debug.LogWarning($"[EquipmentDetailUI] 아이콘 로드에 실패했습니다: {iconAddress}");
            Image_ItemIcon.sprite = null;
            Image_ItemIcon.gameObject.SetActive(false);
            return;
        }

        Image_ItemIcon.sprite = loadedSprite;
        Image_ItemIcon.gameObject.SetActive(true);
    }

    private void OnClickEnhanceButton()
    {
        if (_viewModel == null)
        {
            return;
        }

        bool canOpenEnhance = _viewModel.RequestOpenEnhance();

        if (canOpenEnhance)
        {
            UiManager.Instance.OpenUi<EquipmentEnhanceUI>().Forget();
        }
    }

    private void OnClickDisassembleButton()
    {
        if (_viewModel == null)
        {
            return;
        }

        bool canOpenDisassemble = _viewModel.RequestOpenDisassemble();

        if (canOpenDisassemble)
        {
            UiManager.Instance.OpenUi<EquipmentDisassembleUI>().Forget();
        }
    }

    private void OnClickCloseButton()
    {
        UiManager.Instance.CloseUi<EquipmentDetailUI>();
    }
}