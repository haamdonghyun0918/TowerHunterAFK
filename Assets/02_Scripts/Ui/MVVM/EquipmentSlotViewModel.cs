public class EquipmentSlotViewModel
{
    private readonly EquipmentModel _equipmentModel;

    public EquipmentSlotViewModel(EquipmentModel equipmentModel)
    {
        _equipmentModel = equipmentModel;
    }

    public string UniqueId
    {
        get
        {
            return _equipmentModel.UniqueId;
        }
    }

    public string IconAddress
    {
        get
        {
            return _equipmentModel.IconAddress;
        }
    }

    public int EnhanceLevel
    {
        get
        {
            return _equipmentModel.EnhanceLevel;
        }
    }
}