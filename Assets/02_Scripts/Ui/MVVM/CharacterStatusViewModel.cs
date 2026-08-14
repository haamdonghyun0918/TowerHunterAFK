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

    public int CurrentHp
    {
        get => _characterStatusModel.CurrentHp;

        set
        {
            if (_characterStatusModel.CurrentHp != value)
            {
                _characterStatusModel.CurrentHp = value;
                OnPropertyChanged(nameof(CurrentHp));
                OnPropertyChanged(nameof(HpRatio));
            }
        }
    }

    public int MaxHp
    {
        get => _characterStatusModel.MaxHp;

        set
        {
            if (_characterStatusModel.MaxHp != value)
            {
                _characterStatusModel.MaxHp = value;
                OnPropertyChanged(nameof(MaxHp));
                OnPropertyChanged(nameof(HpRatio));
            }
        }
    }

    public float HpRatio
    {
        get
        {
            if(MaxHp <= 0)
            {
                return 0f;
            }

            return (float)CurrentHp / MaxHp;
        }
    }

    public int CurrentSkillCost
    {
        get => _characterStatusModel.CurrentSkillCost;

        set
        {
            if(_characterStatusModel.CurrentSkillCost != value)
            {
                _characterStatusModel.CurrentSkillCost = value;
                OnPropertyChanged(nameof(CurrentSkillCost));
            }
        }
    }

    public int MaxSkillCost
    {
        get => _characterStatusModel.MaxSkillCost;

        set
        {
            if(_characterStatusModel.MaxSkillCost != value)
            {
                _characterStatusModel.MaxSkillCost = value;
                OnPropertyChanged(nameof(MaxSkillCost));
            }
        }
    }


    public bool IsActive
    {
        get => _characterStatusModel.IsActive;

        set
        {
            if(_characterStatusModel.IsActive != value)
            {
                _characterStatusModel.IsActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
    }

    public bool IsDead
    {
        get => _characterStatusModel.IsDead;
        
        set
        {
            if(_characterStatusModel.IsDead != value)
            {
                _characterStatusModel.IsDead = value;
                OnPropertyChanged(nameof(IsDead));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(SlotIndex));
        OnPropertyChanged(nameof(CharacterId));
        OnPropertyChanged(nameof(CurrentHp));
        OnPropertyChanged(nameof(MaxHp));
        OnPropertyChanged(nameof(HpRatio));
        OnPropertyChanged(nameof(CurrentSkillCost));
        OnPropertyChanged(nameof(MaxSkillCost));
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(IsDead));
    }
}
