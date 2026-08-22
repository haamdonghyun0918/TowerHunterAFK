using UnityEngine;

public class EquipmentDisassembleViewModel : ViewModelBase
{
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

    private string _rewardText;
    public string RewardText
    {
        get => _rewardText;

        set
        {
            if (_rewardText != value)
            {
                _rewardText = value;

                OnPropertyChanged(nameof(RewardText));
            }
        }
    }

}
