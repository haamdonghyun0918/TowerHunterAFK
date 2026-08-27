using System;
using UnityEngine;

public class MainUi : UiBase
{
    [SerializeField] private UiButton _buttonExpedition;
    [SerializeField] private UiButton _buttonHunterInventory;
    [SerializeField] private UiButton _buttonEquipmentInventory;
    [SerializeField] private UiButton _buttonBossRaid;
    [SerializeField] private UiButton _buttonOffLineReward;
    [SerializeField] private UiButton _buttonGacha;

    public static event Action OnBossRaidStart;
    public static event Action OnBossRaidEnd;

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

        if (_buttonOffLineReward)
        {
            _buttonOffLineReward.BindOnClickButtonEvent(OpenOffLineRewardUi);
        }

        if (_buttonGacha)
        {
            _buttonGacha.BindOnClickButtonEvent(OpenGachaUi);
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

        if (Input.GetKeyDown(KeyCode.T))
        {
            OpenOffLineRewardUi();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            OpenGachaUi();
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
        await UiManager.Instance.OpenUi<BossRaidUI>();
    }

    private async void OpenOffLineRewardUi()
    {
        await UiManager.Instance.OpenUi<OffLineRewardUi>();
    }

    public static void TriggerBossRaidStart()
    {
        OnBossRaidStart?.Invoke();
    }

    public static void TriggerBossRaidEnd()
    {
        OnBossRaidEnd?.Invoke();
    }

    public async void OpenGachaUi()
    {
        await UiManager.Instance.OpenUi<GachaUi>();
    }
}