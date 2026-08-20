using UnityEngine;
using TMPro;
using System;

public class HunterSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text _hunterName;
    [SerializeField] private UiButton _hunterSlotButton;

    private int _index;
    private Action<int> _onClickSlot;

    public void SetUp(CharacterData data, int index = 0, Action<int> onClick = null)
    {
        _index = index;
        _onClickSlot = onClick;

        if (data != null)
        {
            _hunterName.text = data.Name;
        }

        else
        {
            _hunterName.text = "";
        }

        if (_hunterSlotButton != null)
        {
            _hunterSlotButton.BindOnClickButtonEvent(OnClickSlot);
        }
    }

    private void OnClickSlot()
    {
        _onClickSlot?.Invoke(_index);
    }


}