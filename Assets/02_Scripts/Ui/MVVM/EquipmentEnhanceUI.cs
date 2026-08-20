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


    private int _equipmentLevel;

    private EquipmentEnhanceViewModel _viewModel;

    private void OnEnable()
    {
        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentEnhanceViewModel();
        Bind();
        Button_EnhanceBtn.onClick.AddListener(OnClick_EnhanceBtn);
        Button_CloseAll.onClick.AddListener(OnClick_CloseAll);
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
        if (_viewModel == null)
        {
            return;
        }
        
        _equipmentLevel = _viewModel.EnhanceLevel;
        Text_EquipmentName.text = $"{_viewModel.ItemName}+{_equipmentLevel}";
        Text_TotalStat.text = $"공격력 : {_viewModel.TotalAtkText}\n방어력 : {_viewModel.TotalDefText}\n"; //속도는 일단 제외
        Text_EnhanceCost.text = $"강화비용\n{_viewModel.CostText}";
        LoadIconAsync().Forget();
    }

    private void OnClick_EnhanceBtn()
    {
        if (_viewModel == null) return;

        string targetUid = _viewModel.TargetEquipmentUniqueId;
        NetworkManager.Instance.EquipmentService.RequestEnhance(targetUid);
    }

    private void OnClick_CloseAll()
    {
        UiManager.Instance.CloseUi<EquipmentEnhanceUI>();
    }

    private async UniTaskVoid LoadIconAsync()
    {
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
}
