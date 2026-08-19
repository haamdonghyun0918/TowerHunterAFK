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

        resourceViewModel.Gold = resourceViewModel.Gold + addGold;
        // 여기서 바로 저장되도록 수정
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGold(resourceViewModel.Gold);
        }

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

        resourceViewModel.Gold = resourceViewModel.Gold - useGold;
        // 여기서 바로 저장되도록 수정
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGold(resourceViewModel.Gold);
        }

        return true;
    }

    public void RequestAddEquipment(string[] equipments)
    {
        if (equipments == null || equipments.Length == 0)
        {
            return;
        }

    }

    public bool RequestUseEquipments(string equipmentId)
    {
        if(string.IsNullOrEmpty(equipmentId))
        {
            return false;
        }

        return true;
    }
}
