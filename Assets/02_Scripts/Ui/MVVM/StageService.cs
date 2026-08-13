
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

    private void CreateStageViewModel()
    {
        var stageModel = new StageModel();
        var stageViewModel = new StageViewModel(stageModel);

        _stageViewModel = stageViewModel;
        _stageModel = stageModel;
    }
}
