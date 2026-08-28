using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossRaidResultPopupUI : UiBase
{
    [SerializeField] private TMP_Text Text_RaidResult;
    [SerializeField] private TMP_Text Text_Desc;
    [SerializeField] private Button Button_ExitBossRaidResultPopup;

    private void OnEnable()
    {
        BindButton();
    }

    private void OnDisable()
    {
        UnBindButton();
    }


    private void BindButton()
    {
        Button_ExitBossRaidResultPopup.onClick.AddListener(OnClick_ExitBossRaidResultPopupButton);
    }

    private void UnBindButton()
    {
        Button_ExitBossRaidResultPopup.onClick.RemoveAllListeners();
    }

    public void Init(bool isVictory, uint rewardDiamond)
    {
        if (isVictory)
        {
            Text_RaidResult.text = "레이드 성공!";
            Text_RaidResult.color = Color.yellow; 
            Text_Desc.text = $"길드의 랭크가 상승했습니다!\n\n보상 : 다이아 {rewardDiamond}개";
        }
        else
        {
            Text_RaidResult.text = "레이드 실패...";
            Text_Desc.text = "파티가 전멸했습니다.\n다음에 다시 도전해주세요.";
        }
    }

    private void OnClick_ExitBossRaidResultPopupButton()
    {
        UiManager.Instance.CloseUi<BossRaidResultPopupUI>();
    }
}
