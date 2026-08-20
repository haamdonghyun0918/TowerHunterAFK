
public class StageService 
{
    private StageViewModel _stageViewModel;

    public int CurrentStage
    {
        get => GetStageViewModel().CurrentStage;
    }

    public int MaxClearedStage
    {
        get => GetStageViewModel().MaxClearedStage;
    }

    public StageViewModel GetStageViewModel()
    {
        if(_stageViewModel == null )
        {
           CreateStageViewModel();
        }
        return _stageViewModel;
    }

    private void CreateStageViewModel()
    {
        StageModel stageModel = new StageModel();

        _stageViewModel = new StageViewModel(stageModel);
    }

    public void SetStageOnLoad(int stage)
    {
        StageViewModel viewModel = GetStageViewModel();

        viewModel.SetCurrentStage(stage);

        if(SaveManager.Instance != null)
        {
            int maxClearedStage = SaveManager.Instance.GetMaxClearedStage();

            viewModel.SetMaxClearedStageOnLoad(maxClearedStage);
        }
        viewModel.InvokeOnceOnInit();
    }

    public void RequestGoNextStage()
    {
        StageViewModel viewModel = GetStageViewModel();

        viewModel.MoveNextStage();

        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveStage(viewModel.CurrentStage);
        }
    }

    public void SetStage(int stage)
    {
        StageViewModel viewModel = GetStageViewModel();

        viewModel.SetCurrentStage(stage);
        
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveStage(viewModel.CurrentStage);
        }
    }

    public void GoToSafeStage()
    {
        int currentStage = CurrentStage;
        int safeStage;

        if(currentStage < 10)
        {
            safeStage = 1;
        }
        else
        {
            safeStage = ((currentStage - 1) / 10) * 10;
        }

        SetStage(safeStage);
    }

    public void UpdateMaxClearedStage(int clearedStage)
    {
        StageViewModel viewModel = GetStageViewModel();

        if(viewModel.TryUpdateMaxClearedStage(clearedStage) == false)
        {
            return;
        }

        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.UpdateMaxClearedStage(viewModel.MaxClearedStage);
        }
    }

}
