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
        Bind();
        Button_Enhance.onClick.AddListener(OnClick_EnhanceBtn);
        Button_Disassemble.onClick.AddListener(OnClick_DisassembleBtn);
        Button_CloseAll.onClick.AddListener(OnClick_CloseBtn);

        UpdateDetailUIAsync().Forget();
    }

    private void OnDisable()
    {
        Unbind();
        Button_Enhance.onClick.RemoveListener(OnClick_EnhanceBtn);
        Button_Disassemble.onClick.RemoveListener(OnClick_DisassembleBtn);
        Button_CloseAll.onClick.RemoveListener(OnClick_CloseBtn);
    }

    private void Bind()
    {
        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentDetailViewModel();
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
        UpdateDetailUIAsync().Forget();
    }

    private async UniTaskVoid UpdateDetailUIAsync()
    {
        if (_viewModel == null) return;

        Text_EquipmentName.text = _viewModel.ItemName;
        Text_TotalStat.text = _viewModel.TotalStatText;

        if (!string.IsNullOrEmpty(_viewModel.ItemIconAddress))
        {
            Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(_viewModel.ItemIconAddress);
            if (loadedSprite != null)
            {
                Image_ItemIcon.sprite = loadedSprite;
            }
        }
    }

    private void OnClick_EnhanceBtn()
    {
        if (_viewModel == null) return;

        string targetUid = _viewModel.TargetEquipmentUniqueId;

        NetworkManager.Instance.EquipmentService.SetEnhanceTarget(targetUid);

        UiManager.Instance.OpenUi<EquipmentEnhanceUI>().Forget();
    }

    private void OnClick_DisassembleBtn()
    {
        //[TODO] EquipmentDisassembleUI 구현 후 주석 해제
        //UiManager.Instance.OpenUi<EquipmentDisassembleUI>().Forget();
    }

    private void OnClick_CloseBtn()
    {
        UiManager.Instance.CloseUi<EquipmentDetailUI>();
    }
}

