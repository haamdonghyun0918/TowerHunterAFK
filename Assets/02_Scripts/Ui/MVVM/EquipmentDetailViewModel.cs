public class EquipmentDetailViewModel : ViewModelBase
{
    private readonly EquipmentService _equipmentService;
    private EquipmentModel _targetEquipment;

    public EquipmentDetailViewModel(EquipmentService equipmentService)
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
                if (_targetEquipment.EnhanceLevel > 0)
                {
                    return $"{_targetEquipment.Name} +{_targetEquipment.EnhanceLevel}";
                }
                else
                {
                    return _targetEquipment.Name;
                }
            }

            return "";
        }
    }

    public string TotalStatText
    {
        get
        {
            if (HasTarget == false)
            {
                return "";
            }

            string totalAtk = _targetEquipment.GetEquipmentTotalAtk().ToString("N0");
            string totalHp = _targetEquipment.GetEquipmentTotalHp().ToString("N0");
            string totalAtkSpeed = _targetEquipment.GetEquipmentTotalAtkSpeed().ToString("N0");
            string totalDef = _targetEquipment.GetEquipmentTotalDef().ToString("N0");

            return $"공격력 : {totalAtk}\n체력 : {totalHp}\n공격속도 : {totalAtkSpeed}\n방어력 : {totalDef}";
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

    public bool RequestOpenEnhance()
    {
        if (HasTarget == false)
        {
            return false;
        }

        return _equipmentService.TrySetEnhanceTarget(_targetEquipment.UniqueId);
    }

    public bool RequestOpenDisassemble()
    {
        if (HasTarget == false)
        {
            return false;
        }

        return _equipmentService.TrySetDisassembleTarget(_targetEquipment.UniqueId);
    }

    public void NotifyEquipmentChanged(string uniqueId)
    {
        if (HasTarget && _targetEquipment.UniqueId == uniqueId)
        {
            NotifyAllChanged();
        }
    }

    private void NotifyAllChanged()
    {
        OnPropertyChanged(string.Empty);
    }
}