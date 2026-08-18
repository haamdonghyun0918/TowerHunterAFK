using UnityEngine;
using TMPro;
using System.ComponentModel;

public class SleepModeUi : UiBase
{
    [Header("Ui Texts")]
    [SerializeField] private TMP_Text Text_Floor;
    [SerializeField] private TMP_Text Text_Gold;
    [SerializeField] private TMP_Text Text_Exp;
    [SerializeField] private TMP_Text Text_Diamond;
    [SerializeField] private TMP_Text Text_MagicStone;

    private StageViewModel _stageViewModel;
    private PlayerResourceViewModel _playerResourceViewModel;

    private void OnEnable()
    {
        Bind();
        UpdateAllViews();
    }

    private void OnDisable()
    {
        UnBind();
    }

    private void Bind()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.StageService != null)
        {
            _stageViewModel = NetworkManager.Instance.StageService.GetStageViewModel();

            if (_stageViewModel != null)
            {
                _stageViewModel.PropertyChanged -= OnStagePropertyChanged;
                _stageViewModel.PropertyChanged += OnStagePropertyChanged;
            }
        }

        if (NetworkManager.Instance != null && NetworkManager.Instance.PlayerResourceService != null)
        {
            _playerResourceViewModel = NetworkManager.Instance.PlayerResourceService.GetPlayerResourceViewModel();

            if (_playerResourceViewModel != null)
            {
                _playerResourceViewModel.PropertyChanged -= OnResourcePropertyChanged;
                _playerResourceViewModel.PropertyChanged += OnResourcePropertyChanged;
            }
        }
    }

    private void UnBind()
    {
        if (_stageViewModel != null)
        {
            _stageViewModel.PropertyChanged -= OnStagePropertyChanged;
        }

        if (_playerResourceViewModel != null)
        {
            _playerResourceViewModel.PropertyChanged -= OnResourcePropertyChanged;
        }
    }

    private void OnStagePropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(StageViewModel.CurrentStage))
        {
            UpdateStageView();
        }
    }

    private void OnResourcePropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(PlayerResourceViewModel.Gold))
        {
            UpdateGoldView();
        }
       
        else if (eventArgs.PropertyName == nameof(PlayerResourceViewModel.Exp))
        {
            UpdateExpView();
        }

        else if (eventArgs.PropertyName == nameof(PlayerResourceViewModel.Diamond))
        {
            UpdateDiamondView();
        }
    }

    private void UpdateAllViews()
    {
        UpdateStageView();
        UpdateGoldView();
        UpdateExpView();
        UpdateDiamondView();
    }
    private void UpdateStageView()
    {
        if (_stageViewModel != null && Text_Floor != null)
        {
            Text_Floor.text = $"{_stageViewModel.CurrentStage} Floor";
        }
    }

    private void UpdateGoldView()
    {
        if (_playerResourceViewModel != null && Text_Gold != null)
        {
            Text_Gold.text = _playerResourceViewModel.Gold.ToString("N0");
        }
    }

    private void UpdateExpView()
    {
        if (_playerResourceViewModel != null && Text_Exp != null)
        {
            Text_Exp.text = _playerResourceViewModel.Exp.ToString("N0");
        }
    }

    private void UpdateDiamondView()
    {
        if (_playerResourceViewModel != null && Text_Diamond != null)
        {
            Text_Diamond.text = _playerResourceViewModel.Diamond.ToString("N0");
        }
    }
}