public class EquipmentEnhanceViewModel : ViewModelBase
{
    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(TargetEquipmentUniqueId));
        OnPropertyChanged(nameof(ItemName));
        OnPropertyChanged(nameof(EnhanceLevelText));
        OnPropertyChanged(nameof(TotalAtkText));
        OnPropertyChanged(nameof(CostText));
    }

    private string _targetEquipmentUniqueId;
    public string TargetEquipmentUniqueId
    {
        get => _targetEquipmentUniqueId;

        set
        {
            if (_targetEquipmentUniqueId != value)
            {
                _targetEquipmentUniqueId = value;

                OnPropertyChanged(nameof(TargetEquipmentUniqueId));
            }
        }
    }

    private string _itemIconAddress;
    public string ItemIconAddress
    {
        get => _itemIconAddress;

        set
        {
            if (_itemIconAddress != value)
            {
                _itemIconAddress = value;

                OnPropertyChanged(nameof(ItemIconAddress));
            }
        }
    }

    private string _itemName;
    public string ItemName
    {
        get => _itemName;

        set
        {
            if (_itemName != value)
            {
                _itemName = value;

                OnPropertyChanged(nameof(ItemName));
            }
        }
    }

    private string _enhanceLevelText;
    public string EnhanceLevelText
    {
        get => _enhanceLevelText;

        set
        {
            if (_enhanceLevelText != value)
            {
                _enhanceLevelText = value;

                OnPropertyChanged(nameof(EnhanceLevelText));
            }
        }
    }

    private string _totalAtkText;
    public string TotalAtkText
    {
        get => _totalAtkText;

        set
        {
            if (_totalAtkText != value)
            {
                _totalAtkText = value;

                OnPropertyChanged(nameof(TotalAtkText));
            }
        }
    }

    private string _costText;
    public string CostText
    {
        get => _costText;

        set
        {
            if (_costText != value)
            {
                _costText = value;

                OnPropertyChanged(nameof(CostText));
            }
        }
    }
}
