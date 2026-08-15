using UnityEngine;

public class ExpeditionViewModel : ViewModelBase
{
    private ExpeditionModel _model;

    public ExpeditionViewModel(ExpeditionModel model)
    {
        _model = model;
    }

    private string _remainTimeString = "00:00:00";
    
    public string RemainTimeString
    {
        get => _remainTimeString;
        set
        {
            if(_remainTimeString != value)
            {
                _remainTimeString = value;
                OnPropertyChanged(nameof(RemainTimeString));
            }
        }
    }

    public bool IsExpeditionStart
    {
        get => _model.IsExpeditionStart;
        set
        {
            if(_model.IsExpeditionStart != value)
            {
                _model.IsExpeditionStart = value;
                OnPropertyChanged(nameof(IsExpeditionStart));
            }
        }
    }

    public bool IsCompleted
    {
        get => _model.IsCompleted;
        set
        {
            if(_model.IsCompleted != value)
            {
                _model.IsCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(RemainTimeString));
        OnPropertyChanged(nameof(IsExpeditionStart));
        OnPropertyChanged(nameof(IsCompleted));
    }
}