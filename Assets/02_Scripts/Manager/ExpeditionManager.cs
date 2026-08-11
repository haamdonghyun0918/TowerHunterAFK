using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }

    [Header("Expedition Setting")]
    [SerializeField] private List<ExpeditionData> _expeditionsList;
    [SerializeField] private ExpeditionData _selectedExpedition;

    [Header("Progress State")]
    [SerializeField] private bool _expeditionStart = false;
    [SerializeField] private bool _isCompleted = false;
    [SerializeField] private DateTime _startTime;

    //TODO: 헌터들의 데이터를 가져와야 함 + 헌터들을 통하여 스쿼드 짜는 로직 추가할 것
    public event Action<ExpeditionData> OnExpeditionSelected;
    public event Action OnExpeditionStarted;
    public event Action OnExpeditionCompleted;
    public event Action<long, string[]> OnRewardClaimed;
    public event Action<int> OnExpeditionLevelNotEnough;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public UniTask Init()
    {
        _expeditionsList = GameDataManager.Instance.GetAllData<ExpeditionData>();
        LoadExpeditionList();

        Debug.Log("ExpeditionManager 호출");
        return UniTask.CompletedTask;
    }

    private void LoadExpeditionList()
    {
        string savedId = SaveManager.Instance.GetOngoingExpeditionId();
        string savedTime = SaveManager.Instance.GetExpeditionStartTime();

        if ((string.IsNullOrEmpty(savedId) == false) && (string.IsNullOrEmpty(savedTime) == false))
        {
            _selectedExpedition = null;
            foreach (ExpeditionData data in _expeditionsList)
            {
                if (data.Id == savedId)
                {
                    _selectedExpedition = data;
                    break;
                }
            }

            if (_selectedExpedition != null)
            {
                if (DateTime.TryParse(savedTime, out DateTime parsedTime))
                {
                    _startTime = parsedTime;
                    _expeditionStart = true;
                    CheckExpeditionCompletion();
                }
            }
        }
    }
    private void Update()
    {
        if (_expeditionStart && (_isCompleted == false))
        {
            CheckExpeditionCompletion();
        }
    }

    public void SelectExpedition(int index)
    {
        if (index >= 0 && index < _expeditionsList.Count)
        {
            ExpeditionData targetExpedition = _expeditionsList[index];
            //TODO: 현재는 플레이어의 레벨을 하드코딩했지만, 플레이어의 경험치와 레벨 로직 구성시 변경할 것
            int currentPlayerLevel = 1;

            if (currentPlayerLevel < targetExpedition.LimitLevel)
            {
                Debug.Log("제한 레벨을 만족하지 못합니다");
                OnExpeditionLevelNotEnough?.Invoke(targetExpedition.LimitLevel);
                return;
            }

            _selectedExpedition = targetExpedition;
            Debug.Log($"{_selectedExpedition.ExpeditionName}을 선택하였습니다.");
            OnExpeditionSelected?.Invoke(_selectedExpedition);
        }
    }

    public void StartExpedition()
    {
        if (_selectedExpedition == null)
        {
            return;
        }
        _expeditionStart = true;
        _startTime = DateTime.Now;

        string timeStr = _startTime.ToString("O");
        SaveManager.Instance.SaveExpeditionStart(_selectedExpedition.Id, timeStr);

        Debug.Log("원정을 보냈습니다.");
        OnExpeditionStarted?.Invoke();
    }

    private void CheckExpeditionCompletion()
    {
        if (_selectedExpedition == null)
        {
            return;
        }

        DateTime currentTime = DateTime.Now;
        TimeSpan passedHours = currentTime - _startTime;

        if (passedHours.TotalHours >= _selectedExpedition.DurationHours)
        {
            _isCompleted = true;
            Debug.Log("원정 시간이 모두 끝났습니다!");
            OnExpeditionCompleted?.Invoke();
        }
    }

    public TimeSpan GetRemainTime()
    {
        if ((_expeditionStart == false) || _isCompleted || (_selectedExpedition == null))
        {
            return TimeSpan.Zero;
        }

        DateTime endTime = _startTime.AddHours(_selectedExpedition.DurationHours);
        TimeSpan remainTime = endTime - DateTime.Now;

        if (remainTime.TotalSeconds < 0)
        {
            return TimeSpan.Zero;
        }

        return remainTime;
    }

    public void ClaimReward()
    {
        if (_isCompleted && _selectedExpedition != null)
        {
            long rewardGold = _selectedExpedition.RewardGold;
            string[] rewardItems = _selectedExpedition.RewardItems;

            if (rewardGold > 0)
            {
                NetworkManager.Instance.PlayerResourceService.RequestAddGold(rewardGold);
            }

            if (rewardItems != null && rewardItems.Length > 0)
            {
                NetworkManager.Instance.PlayerResourceService.RequestAddItem(rewardItems);
            }

            OnRewardClaimed?.Invoke(rewardGold, rewardItems);
            SaveManager.Instance.ClearExpedition();

            _expeditionStart = false;
            _isCompleted = false;
            _selectedExpedition = null;
        }

        else
        {
            Debug.Log("수령할 보상이 존재하지 않습니다.");
        }
    }

    public string GetRemainTimeString()
    {
        // 남은 시간 UI에서 확인할 수 있도록 하는 Get함수
        TimeSpan remaining = GetRemainTime();
        return string.Format("{0:D2}:{1:D2}:{2:D2}", remaining.Hours, remaining.Minutes, remaining.Seconds);
    }
}