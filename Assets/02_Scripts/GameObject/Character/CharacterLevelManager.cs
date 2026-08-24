public class CharacterLevelManager
{
    private int _maxExp = 100000;
    private int _currentExp;
    private int _needExpForLevelUp = 2000;

    public int GetCurrentExp()
    {
        return _currentExp;
    }

    public int GetMaxExp()
    {
        return _maxExp;
    }

    public void AddExp(int amount)
    {
        _currentExp += amount;

        if (_currentExp >= _maxExp)
        {
            _currentExp = _maxExp;
        }
    }

    public int UseExpForLevelUp()
    {
        if (_currentExp < _needExpForLevelUp)
        {
            return 0;
        }

        _currentExp -= _needExpForLevelUp;

        return _needExpForLevelUp;
    }
}
