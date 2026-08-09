public class PlayerResourceViewModel : ViewModelBase
{

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(Gold));
    }

    private long _gold;
    public long Gold
    {
        get => _gold;
        set
        {
            if (_gold != value)
            {
                _gold = value;
                OnPropertyChanged(nameof(Gold));
            }
        }
    }
}
