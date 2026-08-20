using System.ComponentModel;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ExpeditionView : MonoBehaviour
{
    [Header("List Setting")]
    [SerializeField] private Transform _content;
    [SerializeField] private ExpeditionList _expeditionListPrefab;

    [Header("Info Setting")]
    [SerializeField] private TMP_Text _infoName;
    [SerializeField] private TMP_Text _infoDuration;
    [SerializeField] private TMP_Text _infoLevel;

    [Header("Reward Setting")]
    [SerializeField] private TMP_Text _rewardGold;
    [SerializeField] private TMP_Text _rewardDiamond;
    [SerializeField] private TMP_Text _rewardEquipment;

    [Header("Button & Time")]
    [SerializeField] private TMP_Text _remainTime;
    [SerializeField] private UiButton _buttonStart;
    [SerializeField] private UiButton _buttonClaimReward;
    

    private ExpeditionViewModel _expeditionViewModel;
    private List<ExpeditionData> _expeditionList;
    private int _selectedIndex = -1;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (_content == null || _expeditionListPrefab == null)
        {
            Debug.LogError("[ExpeditionView]: 프리팹 연결을 하지 않았거나 프리팹이 생성되는 위치를 연결하지 않았습니다.");
            return;
        }

        if (_remainTime == null)
        {
            Debug.LogError("[ExpeditionView]: RemainTime이 연결되지 않았습니다.");
        }

        if (GameDataManager.Instance != null)
        {
            _expeditionList = GameDataManager.Instance.GetAllData<ExpeditionData>();
            SetExpeditionList();
        }

        else
        {
            Debug.Log("[ExpeditionView]: GameDataManager가 존재하지 않습니다.");
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.ExpeditionService == null)
        {
            Debug.LogError("[ExpeditionView]: NetworkManager 또는 ExpeditionService가 없습니다");
            return;
        }

        if (_buttonStart)
        {
            _buttonStart.BindOnClickButtonEvent(OnClickStartExpedition);
        }

        if (_buttonClaimReward)
        {
            _buttonClaimReward.BindOnClickButtonEvent(OnClickClaimReward);
        }

        _expeditionViewModel = NetworkManager.Instance.ExpeditionService.GetExpeditionViewModel();
        Bind();

        var model = NetworkManager.Instance.ExpeditionService.GetExpeditionModel();

        if (model.SelectedExpedition != null && model.IsExpeditionStart)
        {
            for (int i = 0; i < _expeditionList.Count; i++)
            {
                if (_expeditionList[i].Id == model.SelectedExpedition.Id)
                {
                    _selectedIndex = i;
                    break;
                }
            }
            UpdateInfo(model.SelectedExpedition);
            UpdateReward(model.SelectedExpedition);
        }

        else if (_expeditionList != null && _expeditionList.Count > 0)
        {
            SelectExpedition(0);
        }

        UpdateView();
    }

    private void OnDisable()
    {
        UnBind();
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

    private void SetExpeditionList()
    {
        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < _expeditionList.Count; i++)
        {
            var data = _expeditionList[i];
            var list = Instantiate(_expeditionListPrefab, _content);
            list.SetUp(data, i, SelectExpedition);
        }
    }

    private void SelectExpedition(int index)
    {
        if (index < 0 || index >= _expeditionList.Count)
        {
            return;
        }

        if (_expeditionViewModel != null && _expeditionViewModel.IsExpeditionStart)
        {
            Debug.LogWarning("이미 진행 중인 원정대가 있어 다른 원정대를 선택할 수 없습니다.");
            return;
        }

        if (NetworkManager.Instance.ExpeditionService.TrySelectExpedition(index))
        {
            _selectedIndex = index;
            UpdateInfo(_expeditionList[index]);
            UpdateReward(_expeditionList[index]);
        }
    }

    private void UpdateInfo(ExpeditionData data)
    {
        if (data == null)
        {
            return;
        }

        if (_infoName)
        {
            _infoName.text = data.ExpeditionName;
        }

        if (_infoDuration)
        {
            _infoDuration.text = $"{data.DurationHours} Hours";
        }

        if (_infoLevel)
        {
            _infoLevel.text = $"Lv.{data.LimitLevel}";
        }
    }

    private void UpdateReward(ExpeditionData data)
    {
        if (_rewardGold)
        {
            _rewardGold.text = $"{data.RewardGold} Gold";
        }
        // 다이아몬드는 Json에 추가하고 연결할 것
        if (_rewardDiamond)
        {
            _rewardDiamond.text = "0 Diamond";
        }

        if (_rewardEquipment)
        {
            if (data.RewardEquipments != null && data.RewardEquipments.Length > 0)
            {
                _rewardEquipment.text = string.Join(",", data.RewardEquipments);
            }

            else
            {
                _rewardEquipment.text = "No Equipments";
            }
        }
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

        if(_remainTime != null)
        {
            _remainTime.text = _expeditionViewModel.RemainTimeString;
        }

        UpdateButton();
    }

    private void UpdateButton()
    {
        if (_expeditionViewModel == null)
        {
            Debug.LogError("[ExdeitionView]: ExpeditionViewModel이 존재하지 않습니다.");
            return;
        }

        bool isStarted = _expeditionViewModel.IsExpeditionStart;
        bool isCompleted = _expeditionViewModel.IsCompleted;

        if (_buttonStart)
        {
            _buttonStart.gameObject.SetActive(isStarted == false);
        }

        if (_buttonClaimReward)
        {
            _buttonClaimReward.gameObject.SetActive(isStarted && isCompleted);
        }
    }

    private void OnClickStartExpedition()
    {
        if (_selectedIndex >= 0)
        {
            NetworkManager.Instance.ExpeditionService.RequestStartExpedition();
        }
    }

    private void OnClickClaimReward()
    {
        NetworkManager.Instance.ExpeditionService.RequestClaimReward();
    }
}