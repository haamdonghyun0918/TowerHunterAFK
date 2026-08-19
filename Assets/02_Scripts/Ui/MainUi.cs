using UnityEngine;

public class MainUi : UiBase
{
    [SerializeField] private UiButton _buttonExpedition;

    private void OnEnable()
    {
        if (_buttonExpedition)
        {
            _buttonExpedition.BindOnClickButtonEvent(OpenExpedition);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OpenExpedition();
        }
    }

    private async void OpenExpedition()
    {
        await UiManager.Instance.OpenUi<ExpeditionUi>();
    }
}