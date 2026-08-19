public class PlayerResourceViewModel : ViewModelBase
{
    private PlayerResourceModel _playerResourceModel;

    public PlayerResourceViewModel(PlayerResourceModel playerResourceModel)
    {
        _playerResourceModel = playerResourceModel;
    }

    public long Gold
    {
        get => _playerResourceModel.Gold;

        set
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

        set
        {
            if( _playerResourceModel.Exp != value)
            {
                _playerResourceModel.Exp = value;

                OnPropertyChanged(nameof(Exp));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(Gold));
    }

    public bool IncreaseGold(long amount)
    {
        if(amount <= 0)
        {
            return false;
        }

        SetGold(_playerResourceModel.Gold +  amount);
        return true;
    }

    public bool TryDecreaseGold(long amount)
    {
        if (amount <= 0 || Gold<amount)
        {
            return false;
        }
        SetGold(Gold - amount);
        return true;
    }

    private void SetGold(long value)
    {
        if (_playerResourceModel.Gold == value)
        {
            return;
        }

        _playerResourceModel.Gold = value;
        OnPropertyChanged(nameof(Gold));
    }
}
