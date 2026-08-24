public class EquipmentDisassembleViewModel : ViewModelBase
{
    private readonly EquipmentService _equipmentService;
    private EquipmentModel _targetEquipment;

    public EquipmentDisassembleViewModel(EquipmentService equipmentService)
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

    public long RewardAmount
    {
        get
        {
            if (HasTarget)
            {
                return _equipmentService.GetDisassembleReward(_targetEquipment);
            }

            return 0;
        }
    }

    public string RewardText
    {
        get
        {
            string currencyName = _equipmentService.DisassembleRewardCurrencyName;
            return $"{RewardAmount:N0} {currencyName}";
        }
    }

    public bool CanDisassemble
    {
        get
        {
            if (HasTarget == false)
            {
                return false;
            }

            return _equipmentService.IsEquipmentEquipped(TargetEquipmentUniqueId) == false;
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

    public bool RequestDisassemble()
    {
        if (HasTarget == false)
        {
            return false;
        }

        return _equipmentService.RequestDisassemble(TargetEquipmentUniqueId);
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