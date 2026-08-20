using System;

public class ExpeditionViewModel : ViewModelBase
{
    private readonly ExpeditionModel _model;
    private string _remainTimeString = "00:00:00";

    public ExpeditionViewModel(ExpeditionModel model)
    {
        _model = model;
    }

    public ExpeditionData SelectedExpedition
    {
        get => _model.SelectedExpedition;

        private set
        {
            if(_model.SelectedExpedition == value)
            {
                return;
            }

            _model.SelectedExpedition = value;
            OnPropertyChanged(nameof(SelectedExpedition));
        }
    }
    
    public string RemainTimeString
    {
        get => _remainTimeString;
        private set
        {
            if(_remainTimeString == value)
            {
                return;
            }

            _remainTimeString = value;
            OnPropertyChanged(nameof(RemainTimeString));
        }
    }

    public bool IsExpeditionStart
    {
        get => _model.IsExpeditionStart;
        private set
        {
            if(_model.IsExpeditionStart == value)
            {
                return;
            }

            _model.IsExpeditionStart = value;
            OnPropertyChanged(nameof(IsExpeditionStart));
        }
    }

    public bool IsCompleted
    {
        get => _model.IsCompleted;
        private set
        {
            if(_model.IsCompleted == value)
            {
                return;
            }

            _model.IsCompleted = value;
            OnPropertyChanged(nameof(IsCompleted));
        }
    }

    public DateTime StartTime
    {
        get => _model.StartTime;
        private set
        {
            if(_model.StartTime == value)
            {
                return;
            }

            _model.StartTime = value;
            OnPropertyChanged(nameof(StartTime));
        }
    }

    public bool SelectExpedition(ExpeditionData expedition)
    {
        if(expedition == null || IsExpeditionStart)
        {
            return false;
        }

        SelectedExpedition = expedition;
        return true;
    }

    public bool RestoreExpedition(ExpeditionData expedition, DateTime startTime)
    {
        if(expedition == null)
        {
            return false;
        }

        SelectedExpedition = expedition;
        StartTime = startTime;
        IsCompleted = false;
        IsExpeditionStart = true;
        RemainTimeString = "00:00:00";

        return true;
    }

    public bool TryStartExpedition(DateTime startTime)
    {
        if (SelectedExpedition == null || IsExpeditionStart)
        {
            return false;
        }

        StartTime = startTime;
        IsCompleted = false;
        IsExpeditionStart = true;

        return true;
    }

    public void CompletedExpedition()
    {
        if(IsExpeditionStart == false)
        {
            return;
        }

        IsCompleted = true;
        RemainTimeString = "00:00:00";
    }

    public void ResetExpedition()
    {
        IsExpeditionStart = false;
        IsCompleted = false;
        SelectedExpedition = null;
        StartTime = default;
        RemainTimeString = "00:00:00";
    }

    public void UpdateRemainTime(TimeSpan remainTime)
    {
        RemainTimeString = string.Format("{0:D2}:{1:D2}:{2:D2}", remainTime.Hours, remainTime.Minutes , remainTime.Seconds);
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(SelectedExpedition));
        OnPropertyChanged(nameof(IsExpeditionStart));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(StartTime));
        OnPropertyChanged(nameof(RemainTimeString));
    }
}