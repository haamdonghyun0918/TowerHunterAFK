using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossRaidBattleUI : UiBase
{
    [SerializeField] private Button Button_ExitBossRaid;
    [SerializeField] private TMP_Text Text_BossBanner;
    [SerializeField] private Slider Slider_BossHP;

    private PlayerPartyControllerForBoss _playerParty;
    private MonsterParty _bossParty;
    private Monster _bossMonster;

    private void OnEnable()
    {
        BindButton();
    }

    private void OnDisable()
    {
        UnBindButton();
        if (_bossMonster != null)
        {
            _bossMonster.UnbindOnStatChangedEvent(UpdateBossHpUI);
        }
    }

    private void BindButton()
    {
        Button_ExitBossRaid.onClick.AddListener(OnClick_ExitBossRaidButton);
    }

    private void UnBindButton()
    {
        Button_ExitBossRaid.onClick.RemoveAllListeners();
    }

    public void Init(PlayerPartyControllerForBoss playerParty, MonsterParty bossParty, string bossName)
    {
        _playerParty = playerParty;
        _bossParty = bossParty;
        _bossMonster = _bossParty.GetMonster(0);

        if (_bossMonster != null)
        {
            Text_BossBanner.text = bossName;
            Slider_BossHP.maxValue = _bossMonster.GetMaxHp();
            Slider_BossHP.value = _bossMonster.GetCurrentHp();  
            _bossMonster.BindOnStatChangedEvent(UpdateBossHpUI);
        }
    }

    private void UpdateBossHpUI(int currentHp, int maxHp)
    {
        Slider_BossHP.value = currentHp;
    }

    public void OnClick_ExitBossRaidButton()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ForceStopBossBattle(_playerParty, _bossParty); 
        }

        UiManager.Instance.CloseUi<BossRaidBattleUI>();
        UiManager.Instance.OpenUi<MainUi>().Forget();
    }


}
