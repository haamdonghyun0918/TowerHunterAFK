public class PlayerResourceViewModel : ViewModelBase
{
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
