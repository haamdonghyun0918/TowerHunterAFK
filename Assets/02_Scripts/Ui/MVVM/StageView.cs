using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class StageView : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_CurrentStage;

    private StageViewModel _stageViewModel;

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        if(_stageViewModel != null)
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
        if(Text_CurrentStage == null)
        {
            Debug.LogError("[StageView]: Text_CurrentStage가 연결되지 않았습니다.");
            return;
        }

        if(NetworkManager.Instance == null || NetworkManager.Instance.StageService == null)
        {
            Debug.LogError("[StageView]: StageService가 없습니다.");
            return;
        }

        _stageViewModel = NetworkManager.Instance.StageService.GetStageViewModel();

        Bind();
        UpdateView();
    }

    private void Bind()
    {
        if(_stageViewModel == null)
        {
            return;
        }

        _stageViewModel.PropertyChanged -= OnPropertyChanged;
        _stageViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void UnBind()
    {
        if (_stageViewModel == null)
        {
            return;
        }
        _stageViewModel.PropertyChanged -= OnPropertyChanged;
    }


    private void OnPropertyChanged(object snder, PropertyChangedEventArgs enventArgs)
    {
        if(enventArgs.PropertyName == nameof(StageViewModel.CurrentStage))
        {
            UpdateView();
        }
    }
    private void UpdateView()
    {
        if(_stageViewModel == null || Text_CurrentStage == null)
        {
            return;
        }

        Text_CurrentStage.text = $"{_stageViewModel.CurrentStage}층";
    }
}
