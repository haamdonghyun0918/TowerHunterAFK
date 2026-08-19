using System.ComponentModel;
using TMPro;
using UnityEngine;

public class ExpeditionView : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_RemainTime;

    private ExpeditionViewModel _expeditionViewModel;

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        if(_expeditionViewModel != null)
        {
            Bind();
            UpdateView();
        }
    }

    private void OnDisable()
    {
        UnBind();
    }

    private void Init()
    {
        if(Text_RemainTime == null)
        {
            Debug.LogError("[ExpeditionView]: Text_RemainTime이 연결되지 않았습니다.");
            return;
        }

        if(NetworkManager.Instance == null || NetworkManager.Instance.ExpeditionService == null)
        {
            Debug.LogError("[ExpeditionView]: ExpeditionService가 없습니다.");
            return;
        }

        _expeditionViewModel = NetworkManager.Instance.ExpeditionService.GetExpeditionViewModel();

        Bind();
        UpdateView();
    }

    private void Bind()
    {
        if(_expeditionViewModel == null)
        {
            return;
        }

        _expeditionViewModel.PropertyChanged -= OnPropertyChanged;
        _expeditionViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void UnBind()
    {
        if(_expeditionViewModel == null)
        {
            return;
        }

        _expeditionViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if(eventArgs.PropertyName == nameof(ExpeditionViewModel.RemainTimeString) || eventArgs.PropertyName == nameof(ExpeditionViewModel.IsExpeditionStart) || eventArgs.PropertyName == nameof(ExpeditionViewModel.IsCompleted))
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        if(_expeditionViewModel == null)
        {
            return;
        }

        if(Text_RemainTime != null)
        {
            Text_RemainTime.text = _expeditionViewModel.RemainTimeString;
        }
    }

    private void OnClickStartExpedition(int selectedIndex)
    {
        if(NetworkManager.Instance.ExpeditionService.TrySelectExpedition(selectedIndex) == true)
        {
            NetworkManager.Instance.ExpeditionService.RequestStartExpedition();
        }
    }

    private void OnClickClaimReward()
    {
        NetworkManager.Instance.ExpeditionService.RequestClaimReward();
    }
}