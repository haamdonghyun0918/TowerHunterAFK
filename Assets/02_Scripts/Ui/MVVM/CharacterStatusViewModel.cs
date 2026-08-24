public class CharacterStatusViewModel : ViewModelBase
{
    private readonly CharacterStatusModel _characterStatusModel;

    public CharacterStatusViewModel(CharacterStatusModel model, int slotIndex)
    {
        _characterStatusModel = model;
        _characterStatusModel.SlotIndex = slotIndex;
    }

    public int SlotIndex => _characterStatusModel.SlotIndex;
    public string CharacterId => _characterStatusModel.CharacterId;
    public int CurrentHp => _characterStatusModel.CurrentHp;
    public int MaxHp => _characterStatusModel.MaxHp;
    public int CurrentSkillCost => _characterStatusModel.CurrentSkillCost;
    public int MaxSkillCost => _characterStatusModel.MaxSkillCost;
    public bool IsActive => _characterStatusModel.IsActive;
    public bool IsDead => _characterStatusModel.IsDead;
    public string Name => _characterStatusModel.Name;

    public float HpRatio
    {
        get
        {
            if (MaxHp <= 0)
            {
                return 0f;
            }

            return (float)CurrentHp / MaxHp;
        }
    }

    public void SetCharacterStatus(
        string characterId,
        int currentHp,
        int maxHp,
        int currentSkillCost,
        int maxSkillCost,
        bool isActive,
        bool isDead,
        string name)
    {
        bool characterIdChanged = (CharacterId != characterId);
        bool currentHpChanged = (CurrentHp != currentHp);
        bool maxHpChanged = (MaxHp != maxHp);
        bool currentSkillCostChanged = (CurrentSkillCost != currentSkillCost);
        bool maxSkillCostChanged = (MaxSkillCost != maxSkillCost);
        bool isActiveChanged = (IsActive != isActive);
        bool isDeadChanged = (IsDead != isDead);
        bool isNameChanged = (Name != name);

        _characterStatusModel.CharacterId = characterId;
        _characterStatusModel.CurrentHp = currentHp;
        _characterStatusModel.MaxHp = maxHp;
        _characterStatusModel.CurrentSkillCost = currentSkillCost;
        _characterStatusModel.MaxSkillCost = maxSkillCost;
        _characterStatusModel.IsActive = isActive;
        _characterStatusModel.IsDead = isDead;
        _characterStatusModel.Name = name;

        if (characterIdChanged)
        {
            OnPropertyChanged(nameof(CharacterId));
        }
        if(currentHpChanged)
        {
            OnPropertyChanged(nameof(CurrentHp));
        }
        if(maxHpChanged)
        {
            OnPropertyChanged(nameof(MaxHp));
        }
        if(currentHpChanged || maxHpChanged)
        {
            OnPropertyChanged(nameof(HpRatio));
        }
        if (currentSkillCostChanged)
        {
            OnPropertyChanged(nameof(CurrentSkillCost));
        }
        if(maxSkillCostChanged)
        {
            OnPropertyChanged(nameof(MaxSkillCost));
        }
        if(isActiveChanged)
        {
            OnPropertyChanged(nameof(IsActive));
        }
        if(isDeadChanged)
        {
            OnPropertyChanged(nameof(IsDead));
        }
        if(isNameChanged)
        {
            OnPropertyChanged(nameof(Name));
        }

    }

    public void UpdateHp(int currentHp, int maxHp)
    {
        bool currentHpChanged = (CurrentHp != currentHp);

        bool maxHpChanged = (MaxHp != maxHp);

        bool isDead = currentHp <= 0;
        bool isDeadChanged = (IsDead != isDead);

        _characterStatusModel.CurrentHp = currentHp;
        _characterStatusModel.MaxHp = maxHp;
        _characterStatusModel.IsDead = isDead;

        if (currentHpChanged)
        {
            OnPropertyChanged(nameof(CurrentHp));
        }

        if (maxHpChanged)
        {
            OnPropertyChanged(nameof(MaxHp));
        }

        if (currentHpChanged || maxHpChanged)
        {
            OnPropertyChanged(nameof(HpRatio));
        }

        if (isDeadChanged)
        {
            OnPropertyChanged(nameof(IsDead));
        }
    }

    public void UpdateSkillCost(int currentSkillCost, int maxSkillCost)
    {
        bool currentChanged = (CurrentSkillCost != currentSkillCost);
        bool maxChanged = (MaxSkillCost != maxSkillCost);

        _characterStatusModel.CurrentSkillCost = currentSkillCost;
        _characterStatusModel.MaxSkillCost = maxSkillCost;

        if(currentChanged)
        {
            OnPropertyChanged(nameof(CurrentSkillCost));
        }

        if(maxChanged)
        {
            OnPropertyChanged(nameof(MaxSkillCost));
        }
    }

    public void Reset()
    {
        SetCharacterStatus("", 0, 0, 0, 0, false, false, "");
    }

}
