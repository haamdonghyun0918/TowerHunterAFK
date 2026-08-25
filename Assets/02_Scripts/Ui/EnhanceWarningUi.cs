using UnityEngine;
using System;

public class EnhanceWarningUi : UiBase
{
    [SerializeField] private UiButton _buttonConfirm;
    [SerializeField] private UiButton _buttonCancel;

    public event Action _onConfirmAction;

    private void OnEnable()
    {
        if (_buttonConfirm != null)
        {
            _buttonConfirm.UnBindOnClickButtonEvent(OnClickConfirm);
            _buttonConfirm.BindOnClickButtonEvent(OnClickConfirm);
        }

        if (_buttonCancel != null)
        {
            _buttonCancel.UnBindOnClickButtonEvent(OnClickCancel);
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

        _onConfirmAction = null;
    }

    public void SetUp(Action onConfirm)
    {
        _onConfirmAction = onConfirm;
    }

    private void OnClickConfirm()
    {
        _onConfirmAction?.Invoke();
        UiManager.Instance.CloseUi<EnhanceWarningUi>();
    }

    private void OnClickCancel()
    {
        UiManager.Instance.CloseUi<EnhanceWarningUi>();
        UiManager.Instance.CloseUi<HunterInfoUi>();
    }
}