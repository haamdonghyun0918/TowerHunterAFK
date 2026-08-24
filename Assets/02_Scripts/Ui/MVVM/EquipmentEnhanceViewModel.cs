public class EquipmentEnhanceViewModel : ViewModelBase
{
    private readonly EquipmentService _equipmentService;
    private EquipmentModel _targetEquipment;

    public EquipmentEnhanceViewModel(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public bool HasTarget
    {
        get
        {
            return _targetEquipment != null;
        }
    }

    public string TargetEquipmentUniqueId
    {
        get
        {
            if (HasTarget)
            {
                return _targetEquipment.UniqueId;
            }

            return "";
        }
    }

    public string ItemIconAddress
    {
        get
        {
            if (HasTarget)
            {
                return _targetEquipment.IconAddress;
            }

            return "";
        }
    }

    public string ItemName
    {
        get
        {
            if (HasTarget)
            {
                return _targetEquipment.Name;
            }

            return "";
        }
    }

    public int EnhanceLevel
    {
        get
        {
            if (HasTarget)
            {
                return _targetEquipment.EnhanceLevel;
            }

            return 0;
        }
    }

    public string TotalAtkText
    {
        get
        {
            if (HasTarget == false)
            {
                return "공격력 : 0";
            }

            int totalAtk = _targetEquipment.GetEquipmentTotalAtk();
            return $"공격력 : {totalAtk:N0}";
        }
    }

    public string TotalDefText
    {
        get
        {
            if (HasTarget == false)
            {
                return "방어력 : 0";
            }

            int totalDef = _targetEquipment.GetEquipmentTotalDef();
            return $"방어력 : {totalDef:N0}";
        }
    }

    public string CostText
    {
        get
        {
            if (HasTarget == false)
            {
                return "0 마석";
            }

            long enhanceCost = _equipmentService.GetEnhanceCost(_targetEquipment);
            string currencyName = _equipmentService.EnhanceCostCurrencyName;

            return $"{enhanceCost:N0} {currencyName}";
        }
    }

    public void SetTarget(EquipmentModel equipmentModel)
    {
        _targetEquipment = equipmentModel;
        NotifyAllChanged();
    }

    public void ClearTarget()
    {
        _targetEquipment = null;
        NotifyAllChanged();
    }

    public bool RequestEnhance()
    {
        if (HasTarget == false)
        {
            return false;
        }

        return _equipmentService.RequestEnhance(TargetEquipmentUniqueId);
    }

    public void NotifyEquipmentChanged(string uniqueId)
    {
        if (TargetEquipmentUniqueId == uniqueId)
        {
            NotifyAllChanged();
        }
    }

    private void NotifyAllChanged()
    {
        OnPropertyChanged(string.Empty);
    }
}