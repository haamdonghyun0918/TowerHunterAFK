public class PlayerResourceViewModel : ViewModelBase
{
    private readonly PlayerResourceModel _playerResourceModel;

    public PlayerResourceViewModel(PlayerResourceModel playerResourceModel)
    {
        _playerResourceModel = playerResourceModel;
    }

    public long Gold
    {
        get => _playerResourceModel.Gold;

        private set
        {
            if(_playerResourceModel.Gold != value)
            {
                _playerResourceModel.Gold = value;

                OnPropertyChanged(nameof(Gold));
            }
        }
    }

    public long Exp
    {
        get => _playerResourceModel.Exp;

        private set
        {
            if( _playerResourceModel.Exp != value)
            {
                _playerResourceModel.Exp = value;

                OnPropertyChanged(nameof(Exp));
            }
        }
    }

    public uint Diamond
    {
        get => _playerResourceModel.Diamond;

        private set
        {
            if(_playerResourceModel.Diamond != value)
            {
                _playerResourceModel.Diamond = value;
                OnPropertyChanged(nameof(Diamond));
            }
        }
    }

    public long MagicStone
    {
        get => _playerResourceModel.MagicStone;
        private set
        {
            if(_playerResourceModel.MagicStone != value)
            {
                _playerResourceModel.MagicStone = value;
                OnPropertyChanged(nameof(MagicStone));
            }
        }
    }

    public void SetGoldOnLoad(long gold)
    {
        Gold = gold;
    }

    public void SetExpOnLoad(long exp)
    {
        Exp = exp;
    }

    public void SetDiamondOnLoad(uint diamond)
    {
        Diamond = diamond;
    }
    public void SetMagicStoneOnLoad(long magicStone)
    {
        MagicStone = magicStone;
    }

    public bool TryDecreaseGold(long amount)
    {
        if(amount <= 0 || Gold < amount)
        {
            return false;
        }
        Gold = Gold - amount;
        return true;
    }

    public bool TryIncreaseGold(long amount)
    {
        if(amount <= 0)
        {
            return false;
        }

        Gold = Gold + amount;
        return true;
    }

    public bool TryIncreaseExp(long amount)
    {
        if(amount <= 0)
        {
            return false;
        }

        Exp = Exp + amount;
        return true;
    }


    public bool TryDecreaseExp(long amount)
    {
        if(amount <= 0 ||  Exp < amount)
        {
            return false;
        }
        Exp = Exp - amount;
        return true;
    }

    public bool TryIncreaseDiamond(uint amount)
    {
        if(amount <= 0)
        {
            return false;
        }
        Diamond = Diamond + amount;
        return true;
    }

    public bool TryDecreaseDiamond(uint amount)
    {
        if(amount <= 0 || Diamond < amount)
        {
            return false;
        }
        Diamond = Diamond - amount;
        return true;
    }

    public bool TryDecreaseMagicStone(long amount)
    {
        if (amount <= 0 || MagicStone < amount)
        {
            return false;
        }
        MagicStone = MagicStone - amount;
        return true;
    }

    public bool TryIncreaseMagicStone(long amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        MagicStone = MagicStone + amount;
        return true;
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(Gold));
        OnPropertyChanged(nameof(Exp));
        OnPropertyChanged(nameof(Diamond));
        OnPropertyChanged(nameof(MagicStone));
    }

}
