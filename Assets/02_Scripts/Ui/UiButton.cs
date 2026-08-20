using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UiButton : MonoBehaviour
{
    [SerializeField] private Button Button_Base;
    [SerializeField] private Text Text_Base;
    [SerializeField] private Image Image_Base;
    [SerializeField] private Image Image_Select;

    private void Awake()
    {
        InitUIButton();
        SetDefaultUI();
    }

    private void OnEnable()
    {
        BindOnClickButtonEvent(OnClickSetSelectUI);
    }

    private void OnDisable()
    {
        Button_Base.onClick.RemoveAllListeners();
    }

    private void SetDefaultUI()
    {
        if (Image_Select != null)
        {
            Image_Select.gameObject.SetActive(false);
        }
    }

    private void InitUIButton()
    {
        if (Button_Base != null)
        {
            return;
        }
        var button = this.gameObject.GetComponentInChildren<Button>();
        if (button != null)
        {
            this.Button_Base = button;
        }
    }

    public void BindOnClickButtonEvent(UnityAction onClickCallback)
    {
        if (Button_Base == null) return;

        Button_Base.onClick.AddListener(onClickCallback);

    }

    public void UnBindOnClickButtonEvent(UnityAction onClickCallback)
    {
        if (Button_Base == null) return;

        Button_Base.onClick.RemoveListener(onClickCallback);
    }

    public void ChangeButtonText(string buttonStr)
    {
        if (Text_Base == null) return;
        Text_Base.text = buttonStr;
    }

    private void OnClickSetSelectUI()
    {
        if (Image_Select != null)
        {
            bool currentActive = Image_Select.gameObject.activeSelf;
            Image_Select.gameObject.SetActive(!currentActive);
        }
    }
}