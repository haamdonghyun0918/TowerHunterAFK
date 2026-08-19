public class StageViewModel : ViewModelBase
{
    private readonly StageModel _stageModel;

    public StageViewModel(StageModel StageModel)
    {
        _stageModel = StageModel;
    }

    public int CurrentStage
    {
        get => _stageModel.CurrentStage;

        private set
        {
            if(_stageModel.CurrentStage != value)
            {
                _stageModel.CurrentStage = value;

                OnPropertyChanged(nameof(CurrentStage));
            }
        }
    }

    public int MaxClearedStage
    {
        get => _stageModel.MaxClearedStage;

        private set
        {
            if( _stageModel.MaxClearedStage != value)
            {
                _stageModel.MaxClearedStage = value;
                OnPropertyChanged(nameof(MaxClearedStage));
            }

        }
    }

    public void SetCurrentStage(int stage)
    {
        if(stage < 1)
        {
            stage = 1;
        }

        CurrentStage = stage;
    }

    public void MoveNextStage()
    {
        SetCurrentStage(CurrentStage + 1);
    }

    public void SetMaxClearedStageOnLoad(int stage)
    {
        if(stage < 0)
        {
            stage = 0;
        }

        MaxClearedStage = stage;
    }
    
    public bool TryUpdateMaxClearedStage(int clearedStage)
    {
        if(clearedStage <=  MaxClearedStage)
        {
            return false;
        }
        MaxClearedStage = clearedStage;
        return true;
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(CurrentStage));
        OnPropertyChanged(nameof(MaxClearedStage));
    }
}
