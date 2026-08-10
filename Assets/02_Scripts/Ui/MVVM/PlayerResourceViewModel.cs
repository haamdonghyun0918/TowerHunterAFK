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

    public long EXP
    {
        get => _playerResourceModel.EXP;

        set
        {
            if( _playerResourceModel.EXP != value)
            {
                _playerResourceModel.EXP = value;

                OnPropertyChanged(nameof(EXP));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(Gold));
    }
}
