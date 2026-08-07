using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ExpeditionData
{
    public string expeditionId;
    public string expeditionName;
    public float maxDurationHours;
    public int goldPerHour;
}

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
    [SerializeField] private float _savedGold = 0;

    //TODO: 헌터들의 데이터를 가져와야 함 + 헌터들을 통하여 스쿼드 짜는 로직 추가할 것
    public event Action<ExpeditionData> OnExpeditionSelected;
    public event Action OnExpeditionStarted;
    public event Action OnExpeditionCompleted;
    public event Action<float> OnRewardClaimed;

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

    private void Update()
    {
        if (_expeditionStart && (_isCompleted == false))
        {
            CalculateReward();
        }
    }

    public void SelectExpedition(int index)
    {
        if (index >= 0 && index < _expeditionsList.Count)
        {
            _selectedExpedition = _expeditionsList[index];
            Debug.Log($"{_selectedExpedition.expeditionName}을 선택하였습니다.");
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
        _savedGold = 0;

        Debug.Log("원정을 보냈습니다.");
        OnExpeditionStarted?.Invoke();
    }

    private void CalculateReward()
    {
        if ((_expeditionStart == false) || (_selectedExpedition == null))
        {
            return;
        }

        DateTime currentTime = DateTime.Now;
        TimeSpan passedTime = currentTime - _startTime;
        float passedHours = (float)passedTime.TotalHours;

        if (passedHours >= _selectedExpedition.maxDurationHours)
        {
            passedHours = _selectedExpedition.maxDurationHours;
            _isCompleted = true;
            _savedGold = passedHours * (_selectedExpedition.goldPerHour);

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

        DateTime endTime = _startTime.AddHours(_selectedExpedition.maxDurationHours);
        TimeSpan remainTime = endTime - DateTime.Now;

        if (remainTime.TotalSeconds < 0)
        {
            return TimeSpan.Zero;
        }

        return remainTime;
    }

    public void ClaimReward()
    {
        if (_savedGold > 0)
        {
            SaveManager.Instance.AddGold(_savedGold);
            OnRewardClaimed?.Invoke(_savedGold);
            _savedGold = 0;
            _expeditionStart = false;
            _isCompleted = false;
        }

        else
        {
            Debug.Log("수령할 보상이 존재하지 않습니다.");
        }
    }

    public float GetSavedGold()
    {
        // 쌓인 재화 확인용 Get함수
        return _savedGold;
    }

    public string GetRemainTimeString()
    {
        // 남은 시간 UI에서 확인할 수 있도록 하는 Get함수
        TimeSpan remaining = GetRemainTime();
        return string.Format("{0:D2}:{1:D2}:{2:D2}", remaining.Hours, remaining.Minutes, remaining.Seconds);
    }
}