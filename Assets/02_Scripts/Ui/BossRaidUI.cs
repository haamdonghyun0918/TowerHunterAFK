using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class BossRaidUI : UiBase
{
    [Header("Boss List")]
    [SerializeField] private Transform _content;
    [SerializeField] private BossRaidList _bossRaidListPrefab;

    [Header("Boss Info")]
    [SerializeField] private TMP_Text _infoName;
    [SerializeField] private TMP_Text _infoHp;
    [SerializeField] private TMP_Text _infoLimitLevel;

    [Header("Reward")]
    [SerializeField] private TMP_Text _rewardDiamond;

    [Header("Buttons")]
    [SerializeField] private UiButton _buttonStart;
    [SerializeField] private UiButton _buttonClose;

    [Header("Boss Raid Party Slots")]
    [SerializeField] private HunterSlot[] _bossRaidSlots = new HunterSlot[BossRaidModel.MaxPartySize];

    private BossRaidService _bossRaidService;
    private BossRaidViewModel _bossRaidViewModel;
    private IReadOnlyList<BossData> _bossList;
    private bool _isBossListCreated;

    private void OnEnable()
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.BossRaidService == null)
        {
            Debug.LogError("[BossRaidUI] NetworkManager 또는 BossRaidService가 없습니다.");
            return;
        }

        BindButtons();

        _bossRaidService = NetworkManager.Instance.BossRaidService;
        _bossRaidViewModel = _bossRaidService.GetBossRaidViewModel();

        BindViewModel();
        _bossRaidService.ReloadPartyFromSave();

        _bossList = _bossRaidService.GetBossList();
        CreateBossList();

        if (_bossRaidViewModel.SelectedBoss == null && _bossList.Count > 0)
        {
            _bossRaidService.TrySelectBoss(0);
        }

        UpdateView();
    }

    private void OnDisable()
    {
        UnbindViewModel();
    }

    private void BindButtons()
    {
        if (_buttonStart != null)
        {
            _buttonStart.UnBindOnClickButtonEvent(OnClickStartBossRaid);
            _buttonStart.BindOnClickButtonEvent(OnClickStartBossRaid);
        }

        if (_buttonClose != null)
        {
            _buttonClose.UnBindOnClickButtonEvent(OnClickClose);
            _buttonClose.BindOnClickButtonEvent(OnClickClose);
        }
    }

    private void BindViewModel()
    {
        if (_bossRaidViewModel == null)
        {
            return;
        }

        _bossRaidViewModel.PropertyChanged -= OnPropertyChanged;
        _bossRaidViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void UnbindViewModel()
    {
        if (_bossRaidViewModel == null)
        {
            return;
        }

        _bossRaidViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void CreateBossList()
    {
        if (_isBossListCreated)
        {
            return;
        }

        if (_content == null || _bossRaidListPrefab == null)
        {
            Debug.LogError("[BossRaidUI] Content 또는 BossRaidList 프리팹이 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < _bossList.Count; i++)
        {
            BossRaidList list = Instantiate(_bossRaidListPrefab, _content);
            list.SetUp(_bossList[i], i, OnClickBossList);
        }

        _isBossListCreated = true;
    }

    private void OnClickBossList(int index)
    {
        _bossRaidService.TrySelectBoss(index);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(BossRaidViewModel.SelectedBoss) || eventArgs.PropertyName == nameof(BossRaidViewModel.PartyUids) || eventArgs.PropertyName == nameof(BossRaidViewModel.IsPartyComplete))
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        if (_bossRaidViewModel == null)
        {
            return;
        }

        UpdateBossInfo(_bossRaidViewModel.SelectedBoss);
        UpdatePartySlots();
    }

    private void UpdateBossInfo(BossData bossData)
    {
        if (bossData == null)
        {
            if (_infoName != null) _infoName.text = "-";
            if (_infoHp != null) _infoHp.text = "-";
            if (_infoLimitLevel != null) _infoLimitLevel.text = "-";
            if (_rewardDiamond != null) _rewardDiamond.text = "-";
            return;
        }

        MonsterData monsterData = GameDataManager.Instance.GetData<MonsterData>(bossData.MonsterId);

        if (_infoName != null)
        {
            _infoName.text = monsterData != null ? monsterData.Name : bossData.MonsterId;
        }

        if (_infoHp != null)
        {
            _infoHp.text = monsterData != null ? $"{monsterData.BaseHp:N0}" : "-";
        }

        if (_infoLimitLevel != null)
        {
            _infoLimitLevel.text = $"Lv.{bossData.LimitLevel}";
        }

        if (_rewardDiamond != null)
        {
            _rewardDiamond.text = $"{bossData.RewardDiamond:N0} Diamond";
        }
    }

    private void UpdatePartySlots()
    {
        if (_bossRaidSlots == null || _bossRaidSlots.Length != BossRaidModel.MaxPartySize)
        {
            Debug.LogError("[BossRaidUI] 보스 레이드 슬롯은 정확히 5개가 연결되어야 합니다.");
            return;
        }

        IReadOnlyList<string> partyUids = _bossRaidViewModel.PartyUids;

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            HunterSlot slot = _bossRaidSlots[i];

            if (slot == null)
            {
                continue;
            }

            string uniqueId = partyUids[i];

            if (string.IsNullOrEmpty(uniqueId) == false && SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out CharacterSaveData saveData))
            {
                CharacterData characterData = GameDataManager.Instance.GetData<CharacterData>(saveData.BaseId);
                slot.SetUp(characterData, uniqueId, OnClickRemoveHunter, null);
            }
            else
            {
                slot.SetUp(null, "", OnClickOpenPartySetting, null);
            }
        }
    }

    private void OnClickRemoveHunter(string uniqueId)
    {
        _bossRaidService.TryRemoveHunter(uniqueId);
    }

    private void OnClickOpenPartySetting(string emptyUid)
    {
        UiManager.Instance.OpenUi<BossRaidPartyUi>().Forget();
    }

    private void OnClickStartBossRaid()
    {
        if (_bossRaidService.RequestStartBossRaid() == false)
        {
            return;
        }

        UiManager.Instance.CloseUi<BossRaidUI>();
        MainUi.TriggerBossRaidStart();
    }

    private void OnClickClose()
    {
        _bossRaidService.ReloadPartyFromSave();
        UiManager.Instance.CloseUi<BossRaidUI>();
        MainUi.TriggerBossRaidEnd();
    }
}