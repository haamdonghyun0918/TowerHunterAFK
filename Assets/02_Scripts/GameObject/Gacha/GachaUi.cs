using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUi : UiBase
{
    [SerializeField] private Button Btn_SingleGacha;
    [SerializeField] private Button Btn_MultipleGacha;
    [SerializeField] private TMP_Text Text_Gold;
    [SerializeField] private TMP_Text Text_Diamond;

    private void OnEnable()
    {
        BindButtons();
        SetCurrencyTexts();
    }

    private void BindButtons()
    {
        Btn_SingleGacha.onClick.RemoveListener(OnClickSingleGachaButton);
        Btn_MultipleGacha.onClick.RemoveListener(OnClickMultipleGachaButton);

        Btn_SingleGacha.onClick.AddListener(OnClickSingleGachaButton);
        Btn_MultipleGacha.onClick.AddListener(OnClickMultipleGachaButton);
    }

    private void SetCurrencyTexts()
    {
        Text_Gold.text = SaveManager.Instance.CurrentSaveData.Gold.ToString("N0");
        Text_Diamond.text = SaveManager.Instance.CurrentSaveData.Diamond.ToString("N0");
    }

    private void OnClickSingleGachaButton()
    {
        GachaSystem.Instance.DrawSingleCharacter().Forget();
        SetCurrencyTexts();
    }

    private void OnClickMultipleGachaButton()
    {
        GachaSystem.Instance.DrawMultipleCharacter().Forget();
        SetCurrencyTexts();
    }
}
