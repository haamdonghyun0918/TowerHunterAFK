using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDisassembleUI : UiBase
{
    [SerializeField] private TMP_Text Text_ItemName;
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private TMP_Text Text_DisassembleReward;
    [SerializeField] private Button Button_RequestDisassemble;
    [SerializeField] private Button Button_CloseAll;

    private EquipmentDisassembleViewModel _viewModel;

    private void OnEnable()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[EquipmentDisassembleUI] NetworkManager가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[EquipmentDisassembleUI] EquipmentService가 없습니다.");
            return;
        }

        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentDisassembleViewModel();

        if (_viewModel == null)
        {
            Debug.LogError("[EquipmentDisassembleUI] EquipmentDisassembleViewModel을 가져오지 못했습니다.");
            return;
        }

        BindViewModel();
        BindButtons();
        UpdateDisassembleUI();
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
        Button_RequestDisassemble.onClick.RemoveListener(OnClickDisassembleButton);
        Button_RequestDisassemble.onClick.AddListener(OnClickDisassembleButton);

        Button_CloseAll.onClick.RemoveListener(OnClickCloseButton);
        Button_CloseAll.onClick.AddListener(OnClickCloseButton);
    }

    private void UnbindButtons()
    {
        Button_RequestDisassemble.onClick.RemoveListener(OnClickDisassembleButton);
        Button_CloseAll.onClick.RemoveListener(OnClickCloseButton);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        UpdateDisassembleUI();
    }

    private void UpdateDisassembleUI()
    {
        if (_viewModel == null || _viewModel.HasTarget == false)
        {
            ClearDisassembleUI();
            return;
        }

        Text_ItemName.text = $"{_viewModel.ItemName} +{_viewModel.EnhanceLevel}";

        if (_viewModel.CanDisassemble)
        {
            Text_DisassembleReward.text = $"분해 보상\n{_viewModel.RewardText}";
            Button_RequestDisassemble.interactable = true;
        }
        else
        {
            Text_DisassembleReward.text = "장착 중인 장비는\n분해할 수 없습니다.";
            Button_RequestDisassemble.interactable = false;
        }

        LoadIconAsync().Forget();
    }

    private void ClearDisassembleUI()
    {
        Text_ItemName.text = "";
        Text_DisassembleReward.text = "";
        Image_ItemIcon.sprite = null;
        Image_ItemIcon.gameObject.SetActive(false);
        Button_RequestDisassemble.interactable = false;
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
            Debug.LogError("[EquipmentDisassembleUI] ResourceManager가 없습니다.");
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
            Debug.LogWarning($"[EquipmentDisassembleUI] 아이콘 로드에 실패했습니다: {iconAddress}");
            Image_ItemIcon.sprite = null;
            Image_ItemIcon.gameObject.SetActive(false);
            return;
        }

        Image_ItemIcon.sprite = loadedSprite;
        Image_ItemIcon.gameObject.SetActive(true);
    }

    private void OnClickDisassembleButton()
    {
        if (_viewModel == null || _viewModel.HasTarget == false)
        {
            return;
        }

        if (_viewModel.CanDisassemble == false)
        {
            Debug.LogWarning("[EquipmentDisassembleUI] 장착 중인 장비는 분해할 수 없습니다.");
            return;
        }

        bool isDisassembled = _viewModel.RequestDisassemble();

        if (isDisassembled == false)
        {
            Debug.LogWarning("[EquipmentDisassembleUI] 장비 분해에 실패했습니다.");
            return;
        }

        UiManager.Instance.CloseUi<EquipmentDisassembleUI>();
        UiManager.Instance.CloseUi<EquipmentDetailUI>();
    }

    private void OnClickCloseButton()
    {
        UiManager.Instance.CloseUi<EquipmentDisassembleUI>();
    }
}