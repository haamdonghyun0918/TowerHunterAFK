using UnityEngine;
using TMPro;
using System;

public class ExpeditionList : MonoBehaviour
{
    [SerializeField] private TMP_Text _expeditionName;
    [SerializeField] private UiButton _expeditionListButton;

    private int _index;
    private Action<int> _onClickList;

    public void SetUp(ExpeditionData data, int index, Action<int> onClick)
    {
        _expeditionName.text = data.ExpeditionName;
        _index = index;
        _onClickList = onClick;

        if (_expeditionListButton != null)
        {
            _expeditionListButton.BindOnClickButtonEvent(OnClickExpedition);
        }
        BindButton();
    }

    private void OnEnable()
    {
        BindButton();
    }

    private void BindButton()
    {
        if (_expeditionListButton != null && _onClickList != null)
        {
            _expeditionListButton.UnBindOnClickButtonEvent(OnClickExpedition);
            _expeditionListButton.BindOnClickButtonEvent(OnClickExpedition);
        }
    }

    private void OnClickExpedition()
    {
        _onClickList?.Invoke(_index);
    }

}