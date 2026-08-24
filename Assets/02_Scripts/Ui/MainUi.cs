using System;
using UnityEngine;

public class MainUi : UiBase
{
    [SerializeField] private UiButton _buttonExpedition;
    [SerializeField] private UiButton _buttonHunterInventory;
    [SerializeField] private UiButton _buttonEquipmentInventory;
    [SerializeField] private UiButton _buttonBossRaid;

    public static event Action OnEnterBossRaid;

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

        if (_buttonBossRaid)
        {
            _buttonBossRaid.BindOnClickButtonEvent(OpenBossRaidUi);
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            OpenBossRaidUi();
        }
    }

    private async void OpenExpedition()
    {
        await UiManager.Instance.OpenUi<ExpeditionUi>();
    }

    private async void OpenHunterInventory()
    {
        await UiManager.Instance.OpenUi<HunterInventoryUi>();
    }

    private async void OpenEquipmentInventory()
    {
        await UiManager.Instance.OpenUi<EquipmentInventoryUi>();
    }

    private async void OpenBossRaidUi()
    {
        //await UiManager.Instance.OpenUI<BossRaidUi>();
        OnEnterBossRaid?.Invoke();
    }
}