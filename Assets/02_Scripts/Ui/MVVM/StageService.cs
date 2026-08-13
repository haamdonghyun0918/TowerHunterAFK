
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
        if( _stageViewModel == null )
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
        var stageModel = GetStageViewModel();

        _stageViewModel.CurrentStage = stage;
    }

    public void RequestGoNextStage()
    {
        if(_stageModel == null && _stageViewModel == null && _stageViewModel.CurrentStage < 0)
        {
            return;
        }

        var stageViewModel = GetStageViewModel();

        stageViewModel.CurrentStage++;
    }

}
