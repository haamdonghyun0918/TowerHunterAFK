public class PlayerResourceService 
{
    private PlayerResourceViewModel _playerResourceViewModel;

    public PlayerResourceViewModel GetPlayerResourceViewModel()
    {
        if(_playerResourceViewModel == null)
        {
            CreatePlayerResourceViewModel();
        }
        return _playerResourceViewModel;
    }

    private PlayerResourceViewModel CreatePlayerResourceViewModel()
    {
        var resourceViewModel = new PlayerResourceViewModel();

        _playerResourceViewModel = resourceViewModel;

        return resourceViewModel;
    }

    public void SetGoldOnLoad(long gold)
    {
        var resourceViewModel = GetPlayerResourceViewModel();

        resourceViewModel.Gold = gold;
    }

    public void RequestAddGold(long addGold)
    {
        if(addGold <= 0)
        {
            return;
        }

        var resourceViewModel = GetPlayerResourceViewModel();

        resourceViewModel.Gold += addGold;

        SaveManager.Instance.SaveGold(resourceViewModel.Gold);
    }
}
