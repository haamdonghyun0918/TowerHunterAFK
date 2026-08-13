
public class StageService 
{
    private StageModel _stageModel;
    private StageViewModel _stageViewModel;

    public StageViewModel GetStageViewModel()
    {
        if(_stageViewModel == null )
        {
           CreateStageViewModel();
        }
        return _stageViewModel;
    }

    public StageModel GetStageModel()
    {
        if (_stageModel == null || _stageViewModel == null)
        {
            CreateStageViewModel();
        }
        return _stageModel;
    }

    private void CreateStageViewModel()
    {
        var stageModel = new StageModel();
        var stageViewModel = new StageViewModel(stageModel);

        _stageViewModel = stageViewModel;
        _stageModel = stageModel;
    }

    public void SetStageOnLoad(int stage)
    {
        var stageViewModel = GetStageViewModel();

        if(stage<1)
        {
            stage = 1;
        }

        stageViewModel.CurrentStage = stage;

        if (SaveManager.Instance != null)
        {
            stageViewModel.MaxClearedStage = SaveManager.Instance.GetMaxClearedStage();
        }

        stageViewModel.InvokeOnceOnInit();
    }

    public void RequestGoNextStage()
    {
        var stageViewModel = GetStageViewModel();

        if(stageViewModel.CurrentStage < 1)
        {
            return;
        }

        SetStage(stageViewModel.CurrentStage + 1);
    }

    public void SetStage(int stage)
    {
        var stageViewModel = GetStageViewModel();

        if(stage < 1)
        {
            stage = 1;
        }

        stageViewModel.CurrentStage = stage;

        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveStage(stage);
        }
    }

    public void GoToSafeStage()
    {
        var stageViewModel = GetStageViewModel();

        int currentStage = stageViewModel.CurrentStage;
        int safeStage = ((currentStage - 1) / 10) * 10;

        if( stageViewModel == null )
        {
            return;
        }
        if(currentStage < 10)
        {
            safeStage = 1;
        }
        SetStage(safeStage);
    }

    public void UpdateMaxClearedStage(int clearedStage)
    {
        var stageViewModel = GetStageViewModel();

        if(clearedStage <= stageViewModel.CurrentStage)
        {
            return;
        }

        stageViewModel.MaxClearedStage = clearedStage;

        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.UpdateMaxClearedStage(clearedStage);
        }
    }

}
