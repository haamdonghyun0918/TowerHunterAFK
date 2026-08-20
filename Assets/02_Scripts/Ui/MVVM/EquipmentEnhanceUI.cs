using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentEnhanceUI : UiBase
{
    [SerializeField] private TMP_Text Text_EquipmentName;
    [SerializeField] private TMP_Text Text_TotalAtk;
    [SerializeField] private TMP_Text Text_TotalDef;
    [SerializeField] private TMP_Text Text_EnhanceCost;
    [SerializeField] private Button Button_EnhanceBtn;

    private int _equipmentLevel;

    private EquipmentEnhanceViewModel _viewModel;

    private void OnEnable()
    {
        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentEnhanceViewModel();
        Bind();
        Button_EnhanceBtn.onClick.AddListener(OnClick_EnhanceBtn);
        UpdateEnhanceUI();
    }

    private void OnDisable()
    {
        Unbind();
        Button_EnhanceBtn.onClick.RemoveListener(OnClick_EnhanceBtn);
    }

    private void Bind()
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.PropertyChanged -= OnPropertyChanged;
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void Unbind()
    {
        if ( _viewModel == null)
        {
            return;
        }

        _viewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateEnhanceUI();
    }

    private void UpdateEnhanceUI()
    {
        if (_viewModel == null) return;

        Text_EquipmentName.text = _viewModel.ItemName;
        _equipmentLevel = _viewModel.EnhanceLevel;
        Text_TotalAtk.text = _viewModel.TotalAtkText;
        Text_TotalDef.text = _viewModel.TotalDefText;
        Text_EnhanceCost.text = _viewModel.CostText;
    }

    private void OnClick_EnhanceBtn()
    {
        if (_viewModel == null) return;

        string targetUid = _viewModel.TargetEquipmentUniqueId;
        NetworkManager.Instance.EquipmentService.RequestEnhance(targetUid);
    }
}
