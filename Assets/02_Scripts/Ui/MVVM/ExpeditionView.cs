using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

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

    [Header("Hunter Party Slots")]
    [SerializeField] private HunterSlot[] _expeditionSlots = new HunterSlot[3];

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

        ExpeditionPartySetting.OnPartyChanged += UpdateExpeditionPartySlots;

        ExpeditionData selectedExpedition = _expeditionViewModel.SelectedExpedition;

        if (selectedExpedition != null && _expeditionViewModel.IsExpeditionStart)
        {
            if(_expeditionList != null)
            {
                for (int i = 0; i < _expeditionList.Count; i++)
                {
                    if (_expeditionList[i].Id == selectedExpedition.Id)
                    {
                        _selectedIndex = i;
                        break;
                    }
                }
            }
            
            UpdateInfo(selectedExpedition);
            UpdateReward(selectedExpedition);
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
        ExpeditionPartySetting.OnPartyChanged -= UpdateExpeditionPartySlots;
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
        if(data == null)
        {
            return;
        }

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
                List<string> equipNames = new List<string>();
                foreach (string equipId in data.RewardEquipments)
                {
                    EquipmentData equipData = GameDataManager.Instance.GetData<EquipmentData>(equipId);

                    if (equipData != null)
                    {
                        equipNames.Add(equipData.Name);
                    }

                    else
                    {
                        equipNames.Add(equipId);
                    }
                }
                _rewardEquipment.text = string.Join(", ", equipNames);
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
        UpdateExpeditionPartySlots();
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

    private void UpdateExpeditionPartySlots()
    {
        string[] expParty = SaveManager.Instance.CurrentSaveData.ExpeditionPartyUids;
        bool isStarted = _expeditionViewModel.IsExpeditionStart;

        for (int i = 0; i < 3; i++)
        {
            if (_expeditionSlots[i] == null) continue;

            string uid = expParty[i];

            if (string.IsNullOrEmpty(uid) == false && SaveManager.Instance.CharacterDict.TryGetValue(uid, out var charData))
            {
                var data = GameDataManager.Instance.GetData<CharacterData>(charData.BaseId);
                Action<string> onClickAction = isStarted ? (Action<string>)null : OnClickExpeditionSlotRemove;
                _expeditionSlots[i].SetUp(data, uid, onClickAction, null);
            }

            else
            {
                Action<string> onClickAction = isStarted ? (Action<string>)null : OnClickExpeditionSlotAdd;
                _expeditionSlots[i].SetUp(null, "", onClickAction, null);
            }
        }
    }

    private void OnClickExpeditionSlotRemove(string uid)
    {
        ExpeditionPartySetting setting = new ExpeditionPartySetting();
        if (setting.RemoveCharacterFromExpedition(uid))
        {
            UpdateView();
        }
    }

    private void OnClickExpeditionSlotAdd(string emptyUid)
    {
        UiManager.Instance.OpenUi<ExpeditionPartyUi>().Forget();
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