public class PlayerResourceService 
{
    private PlayerResourceModel _playerResourceModel;

    private PlayerResourceViewModel _playerResourceViewModel;

    public PlayerResourceViewModel GetPlayerResourceViewModel()
    {
        if(_playerResourceViewModel == null)
        {
            CreatePlayerResourceViewModel();
        }
        return _playerResourceViewModel;
    }

    public PlayerResourceModel GetPlayerResourceModel()
    {
        if(_playerResourceModel == null)
        {
            CreatePlayerResourceViewModel();
        }

        return _playerResourceModel;
    }

    private void CreatePlayerResourceViewModel()
    {
        var resourceModel = new PlayerResourceModel();

        var resourceViewModel = new PlayerResourceViewModel(resourceModel);

        _playerResourceModel = resourceModel;
        _playerResourceViewModel = resourceViewModel;
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

    }

    public bool RequestUseGold(long useGold)
    {
        if(useGold <= 0)
        {
            return false;
        }

        var resourceViewModel = GetPlayerResourceViewModel();

        if(resourceViewModel.Gold  < useGold)
        {
            return false;
        }

        resourceViewModel.Gold -= useGold;

        return true;
    }

    public void RequestAddItem(string[] items)
    {
        if (items == null || items.Length == 0)
        {
            return;
        }

    }

    public bool RequestUseItem(string item)
    {
        if(string.IsNullOrEmpty(item))
        {
            return false;
        }

        return true;
    }
}
