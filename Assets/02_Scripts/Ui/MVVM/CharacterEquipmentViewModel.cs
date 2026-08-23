public class CharacterEquipmentViewModel : ViewModelBase
{
    private readonly EquipmentService _equipmentService;
    private string _characterUniqueId = "";

    public CharacterEquipmentViewModel(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public bool HasTarget
    {
        get
        {
            return string.IsNullOrEmpty(_characterUniqueId) == false;
        }
    }

    public string WeaponIconAddress
    {
        get
        {
            return GetEquipmentIconAddress(EquipmentSlot.Weapon);
        }
    }

    public string ArmorIconAddress
    {
        get
        {
            return GetEquipmentIconAddress(EquipmentSlot.Armor);
        }
    }

    public string AccessoryIconAddress
    {
        get
        {
            return GetEquipmentIconAddress(EquipmentSlot.Accessory);
        }
    }

    public EquipmentStatBonus TotalBonus
    {
        get
        {
            if (HasTarget)
            {
                return _equipmentService.GetCharacterEquipmentStatBonus(_characterUniqueId);
            }

            return new EquipmentStatBonus();
        }
    }

    public void SetCharacterTarget(string characterUniqueId)
    {
        _characterUniqueId = characterUniqueId;
        NotifyAllChanged();
    }

    public bool RequestBeginEquip(EquipmentSlot slot)
    {
        if (HasTarget == false)
        {
            return false;
        }

        return _equipmentService.BeginEquipSelection(_characterUniqueId, slot);
    }

    public void NotifyCharacterEquipmentChanged(string characterUniqueId)
    {
        if (_characterUniqueId == characterUniqueId)
        {
            NotifyAllChanged();
        }
    }

    public void NotifyAllChanged()
    {
        OnPropertyChanged(string.Empty);
    }

    private string GetEquipmentIconAddress(EquipmentSlot slot)
    {
        if (HasTarget == false)
        {
            return "";
        }

        EquipmentModel equipmentModel = _equipmentService.GetEquippedEquipmentModel(_characterUniqueId, slot);

        if (equipmentModel == null)
        {
            return "";
        }

        return equipmentModel.IconAddress;
    }
}