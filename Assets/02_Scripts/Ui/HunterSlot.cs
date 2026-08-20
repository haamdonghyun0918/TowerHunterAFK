using UnityEngine;
using TMPro;
using System;

public class HunterSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text _hunterName;
    [SerializeField] private UiButton _hunterSlotButton;

    private string _uniqueId;
    private Action<string> _onClickSlot;

    public void SetUp(CharacterData data, string uniqueId, Action<string> onClick = null)
    {
        _uniqueId = uniqueId;
        _onClickSlot = onClick;

        if (data != null)
        {
            _hunterName.text = data.Name;
        }

        else
        {
            _hunterName.text = "비어있음";
        }

        if (_hunterSlotButton != null)
        {
            _hunterSlotButton.UnBindOnClickButtonEvent(OnClickSlot);
            _hunterSlotButton.BindOnClickButtonEvent(OnClickSlot);
        }
    }

    private void OnClickSlot()
    {
        _onClickSlot?.Invoke(_uniqueId);
    }


}