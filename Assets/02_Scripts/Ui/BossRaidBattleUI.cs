using UnityEngine;
using UnityEngine.UI;

public class BossRaidBattleUI : UiBase
{
    [SerializeField] private Button Button_ExitBossRaid;

    private void OnEnable()
    {
        BindButton();
    }

    private void BindButton()
    {
        Button_ExitBossRaid.onClick.AddListener(OnClick_RxitBossRaidButton);
    }


    public void OnClick_RxitBossRaidButton()
    {
        UiManager.Instance.CloseUi<BossRaidBattleUI>();
        UiManager.Instance.OpenUi<MainUi>();
    }


}
