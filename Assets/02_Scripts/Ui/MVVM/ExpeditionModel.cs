using System;

public class ExpeditionModel
{
    public ExpeditionData SelectedExpedition { get; set; }
    public bool IsExpeditionStart { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public DateTime StartTime { get; set; }
}