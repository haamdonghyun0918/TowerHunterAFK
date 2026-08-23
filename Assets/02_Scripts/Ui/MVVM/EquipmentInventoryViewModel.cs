using System.Collections.Generic;

public enum EquipmentInventoryMode
{
    Browse = 0,
    Equip = 1
}

public enum EquipmentSelectionResult
{
    Failed = 0,
    OpenDetail = 1,
    Equipped = 2
}

public class EquipmentInventoryViewModel : ViewModelBase
{
    private readonly EquipmentService _equipmentService;
    private readonly List<EquipmentSlotViewModel> _equipments = new List<EquipmentSlotViewModel>();

    private EquipmentInventoryMode _mode;
    private string _targetCharacterUniqueId = "";
    private EquipmentSlot _targetSlot;

    public EquipmentInventoryViewModel(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public IReadOnlyList<EquipmentSlotViewModel> Equipments
    {
        get
        {
            return _equipments;
        }
    }

    public void SetBrowseMode()
    {
        _mode = EquipmentInventoryMode.Browse;
        _targetCharacterUniqueId = "";
        _targetSlot = EquipmentSlot.None;
        Refresh();
    }

    public void SetEquipMode(string characterUniqueId, EquipmentSlot slot)
    {
        _mode = EquipmentInventoryMode.Equip;
        _targetCharacterUniqueId = characterUniqueId;
        _targetSlot = slot;
        Refresh();
    }

    public void Refresh()
    {
        _equipments.Clear();

        IReadOnlyList<EquipmentModel> equipmentModels = _equipmentService.GetOwnedEquipmentModels();

        for (int i = 0; i < equipmentModels.Count; i++)
        {
            EquipmentModel equipmentModel = equipmentModels[i];

            if (equipmentModel == null)
            {
                continue;
            }

            if (IsEquipMode())
            {
                if (equipmentModel.Slot != _targetSlot)
                {
                    continue;
                }

                bool canEquip = _equipmentService.CanEquip(_targetCharacterUniqueId, equipmentModel.UniqueId, out _);
                bool isAlreadyEquippedByTarget = _equipmentService.IsEquipmentEquippedByCharacter(_targetCharacterUniqueId, equipmentModel.UniqueId);

                if (canEquip == false && isAlreadyEquippedByTarget == false)
                {
                    continue;
                }
            }

            EquipmentSlotViewModel itemViewModel = new EquipmentSlotViewModel(equipmentModel);
            _equipments.Add(itemViewModel);
        }

        OnPropertyChanged(nameof(Equipments));
    }

    public EquipmentSelectionResult RequestSelectEquipment(string uniqueId)
    {
        if (IsEquipMode())
        {
            bool isEquipped = _equipmentService.RequestEquip(_targetCharacterUniqueId, uniqueId);

            if (isEquipped == false)
            {
                return EquipmentSelectionResult.Failed;
            }

            SetBrowseMode();
            return EquipmentSelectionResult.Equipped;
        }

        bool isDetailTargetSet = _equipmentService.TrySetDetailTarget(uniqueId);

        if (isDetailTargetSet)
        {
            return EquipmentSelectionResult.OpenDetail;
        }

        return EquipmentSelectionResult.Failed;
    }

    public void RequestCancelEquipSelection()
    {
        if (IsEquipMode())
        {
            SetBrowseMode();
        }
    }

    private bool IsEquipMode()
    {
        return _mode == EquipmentInventoryMode.Equip;
    }
}