using UnityEngine;

public class MainUi : UiBase
{
    [SerializeField] private UiButton _buttonExpedition;
    [SerializeField] private UiButton _buttonHunterInventory;
    [SerializeField] private UiButton _buttonEquipmentInventory;


    private void OnEnable()
    {
        if (_buttonExpedition)
        {
            _buttonExpedition.BindOnClickButtonEvent(OpenExpedition);
        }

        if (_buttonHunterInventory)
        {
            _buttonHunterInventory.BindOnClickButtonEvent(OpenHunterInventory);
        }

        if ( _buttonEquipmentInventory)
        {
            _buttonEquipmentInventory.BindOnClickButtonEvent(OpenEquipmentInventory);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OpenExpedition();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            OpenHunterInventory();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenEquipmentInventory();
        }
    }

    private async void OpenExpedition()
    {
        await UiManager.Instance.OpenUi<ExpeditionUi>();
    }

    private async void OpenHunterInventory()
    {
        //await UiManager.Instance.OpenUi<HunterInventory>
    }

    private async void OpenEquipmentInventory()
    {
        //await UiManager.Instance.OpenUi<EquipmentInventory>
    }
}