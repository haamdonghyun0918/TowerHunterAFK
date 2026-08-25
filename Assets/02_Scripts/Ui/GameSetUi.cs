using UnityEngine;

public class GameSetUi : UiBase
{
    [SerializeField] private UiButton _buttonConfirm;
    [SerializeField] private UiButton _buttonCancel;

    private void OnEnable()
    {
        if (_buttonConfirm != null)
        {
            _buttonConfirm.BindOnClickButtonEvent(OnClickConfirm);
        }

        if (_buttonCancel != null)
        {
            _buttonCancel.BindOnClickButtonEvent(OnClickCancel);
        }
    }

    private void OnDisable()
    {
        if (_buttonConfirm != null)
        {
            _buttonConfirm.UnBindOnClickButtonEvent(OnClickConfirm);
        }

        if (_buttonCancel != null)
        {
            _buttonCancel.UnBindOnClickButtonEvent(OnClickCancel);
        }
    }

    private void OnClickConfirm()
    {
        Application.Quit();
    }

    private void OnClickCancel()
    {
        if (UiManager.Instance != null)
        {
            UiManager.Instance.CloseUi<GameSetUi>();
        }
    }

}