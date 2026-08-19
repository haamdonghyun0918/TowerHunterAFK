using UnityEngine;

public class EquipmentInventoryUi : UiBase
{
    [SerializeField] private UiButton _buttonClose;

    private void OnEnable()
    {
        if (_buttonClose)
        {
            _buttonClose.BindOnClickButtonEvent(CloseEquipmentInventory);
        }
    }

    private void CloseEquipmentInventory()
    {
        UiManager.Instance.CloseUi<EquipmentInventoryUi>();
    }
}