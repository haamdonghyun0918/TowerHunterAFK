public class StageViewModel : ViewModelBase
{
    private StageModel _stageModel;

    public StageViewModel(StageModel StageModel)
    {
        _stageModel = StageModel;
    }

    public int CurrentStage
    {
        get => _stageModel.CurrentStage;

        set
        {
            if(_stageModel.CurrentStage != value)
            {
                _stageModel.CurrentStage = value;

                OnPropertyChanged(nameof(CurrentStage));
            }
        }
    }
}
