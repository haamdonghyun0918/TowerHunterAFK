using UnityEngine;

public class ExpeditionUi : UiBase
{
    [SerializeField] private UiButton _buttonClose;

    private void OnEnable()
    {
        if (_buttonClose)
        {
            _buttonClose.BindOnClickButtonEvent(CloseExpedition);
        }
    }

    private void CloseExpedition()
    {
        UiManager.Instance.CloseUi<ExpeditionUi>();
    }
}