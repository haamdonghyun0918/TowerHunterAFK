using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterStatusViewModel : ViewModelBase
{
    private CharacterStatusModel _characterStatusModel;

    public CharacterStatusViewModel(CharacterStatusModel characterStatusModel)
    {
        _characterStatusModel = characterStatusModel;
    }

    public int SlotIndex
    {
        get => _characterStatusModel.SlotIndex;

        set
        {
            if(_characterStatusModel.SlotIndex != value)
            {
                _characterStatusModel.SlotIndex = value;
                OnPropertyChanged(nameof(SlotIndex));
            }
        }
    }
    public string CharacterId
    {
        get => _characterStatusModel.CharacterId;

        set
        {
            if(_characterStatusModel.CharacterId !=value)
            {
                _characterStatusModel.CharacterId = value;
                OnPropertyChanged(nameof(CharacterId));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(SlotIndex));
        OnPropertyChanged(nameof(CharacterId));
    }
}
