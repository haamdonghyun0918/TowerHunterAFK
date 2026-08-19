using static UnityEditor.Profiling.HierarchyFrameDataView;

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
    
    private void CreatePlayerResourceViewModel()
    {
        var resourceModel = new PlayerResourceModel();

        var resourceViewModel = new PlayerResourceViewModel(resourceModel);

    }

    public void SetGoldOnLoad(long gold)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        viewModel.SetGoldOnLoad(gold);
    }

    public void SetExpOnLoad(long exp)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        viewModel.SetExpOnLoad(exp);
    }

    public void SetDiamondOnLoad(uint diamond)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        viewModel.SetDiamondOnLoad(diamond);
    }

    public void RequestAddGold(long addGold)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        if(viewModel.TryIncreaseGold(addGold) == false)
        {
            return;
        }

        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGold(viewModel.Gold);
        }

    }

    public void RequestAddDiamond(uint addDiamond)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        if (viewModel.TryIncreaseDiamond(addDiamond) == false)
        {
            return;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveDiamond(viewModel.Diamond);
        }
    }

    public void RequestAddExp(long addExp)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        if( viewModel.TryIncreaseExp(addExp) == false)
        {
            return;
        }
        if( SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveExp(viewModel.Exp);
        }
    }

    public bool RequestUseGold(long useGold)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        if(viewModel.TryDecreaseGold(useGold) == false)
        {
            return false;
        }

        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGold(viewModel.Gold);
        }
        return true;
    }

    public bool RequestUseExp(long useExp)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        if (viewModel.TryDecreaseExp(useExp) == false)
        {
            return false;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveExp(viewModel.Exp);
        }
        return true;
    }

    public bool RequestUseDiamond(uint useDiamond)
    {
        PlayerResourceViewModel viewModel = GetPlayerResourceViewModel();

        if (viewModel.TryDecreaseDiamond(useDiamond) == false)
        {
            return false;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveDiamond(viewModel.Diamond);
        }
        return true;
    }




    //ToDo 장비 아이템 데이터
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
