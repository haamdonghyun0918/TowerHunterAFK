using UnityEngine;
using UnityEngine.UI;

public class BossRaidBattleUI : UiBase
{
    [SerializeField] private Button Button_ExitBossRaid;

    private void Awake()
    {
        
    }

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
        //BattleManager.Instance.EndBossBattle();
        UiManager.Instance.CloseUi<BossRaidBattleUI>();
        UiManager.Instance.OpenUi<MainUi>();
        //보스 배틀 끝내는 로직 실행시킬것.


    }


}
