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
        Bind();
        Button_RequestDisassemble.onClick.AddListener(OnClick_RequestDisassembleButton);
        Button_CloseAll.onClick.AddListener(OnClick_CloseAll);
        UpdateDisassembleUI();
    }

    private void OnDisable()
    {
        Unbind();
        Button_RequestDisassemble.onClick.RemoveListener(OnClick_RequestDisassembleButton);
        Button_CloseAll.onClick.RemoveListener(OnClick_CloseAll);
    }

    private void Bind()
    {
        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentDisassembleViewModel();

        if (_viewModel == null)
        {
            return;
        }

        _viewModel.PropertyChanged -= OnPropertyChanged;
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void Unbind()
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateDisassembleUI();
    }

    private void UpdateDisassembleUI()
    {
        if (_viewModel == null)
        {
            return;
        }

        Text_ItemName.text = $"{_viewModel.ItemName}";
        Text_DisassembleReward.text = $"{_viewModel.RewardText}";
        LoadIconAsync().Forget();
    }

    private async UniTaskVoid LoadIconAsync()
    {
        if (string.IsNullOrEmpty(_viewModel.ItemIconAddress))
        {
            Debug.LogWarning("[EquipmentEnhanceUI] 아이콘 주소가 Null이어서 로드를 생략합니다.");
            return;
        }

        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(_viewModel.ItemIconAddress);

        if (loadedSprite != null)
        {
            Image_ItemIcon.sprite = loadedSprite;
        }
        else
        {
            Debug.LogWarning($"[HunterSlot] 아이콘 로드 실패: {_viewModel.ItemIconAddress}");
        }
    }

    private void OnClick_RequestDisassembleButton()
    {
        if (_viewModel == null)
        {
            return;
        }

        string targetUid = _viewModel.TargetEquipmentUniqueId;
        bool isDisassembleSuccess = NetworkManager.Instance.EquipmentService.RequestDisassemble(targetUid);

        if (isDisassembleSuccess)
        {
            OnClick_CloseAll();
            UiManager.Instance.CloseUi<EquipmentDetailUI>();
        }
    }

    private void OnClick_CloseAll()
    {
        UiManager.Instance.CloseUi<EquipmentDisassembleUI>();
    }
}
