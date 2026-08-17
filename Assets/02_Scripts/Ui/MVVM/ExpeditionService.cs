using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ExpeditionService
{
    private ExpeditionModel _expeditionModel;
    private ExpeditionViewModel _expeditionViewModel;
    private List<ExpeditionData> _expeditionsList;

    public ExpeditionViewModel GetExpeditionViewModel()
    {
        if(_expeditionModel == null)
        {
            CreateExpeditionViewModel();
        }
        return _expeditionViewModel;
    }

    public ExpeditionModel GetExpeditionModel()
    {
        if(_expeditionModel == null || _expeditionViewModel == null)
        {
            CreateExpeditionViewModel();
        }
        return _expeditionModel;
    }

    private void CreateExpeditionViewModel()
    {
        var expeditionModel = new ExpeditionModel();
        var expeditionViewModel = new ExpeditionViewModel(expeditionModel);

        _expeditionModel = expeditionModel;
        _expeditionViewModel = expeditionViewModel;
    }

    public void Init()
    {
        GetExpeditionViewModel();

        _expeditionsList = GameDataManager.Instance.GetAllData<ExpeditionData>();
        LoadExpeditionList();
    }

    private void LoadExpeditionList()
    {
        string savedId = SaveManager.Instance.GetOngoingExpeditionId();
        string savedTime = SaveManager.Instance.GetExpeditionStartTime();

        if(string.IsNullOrEmpty(savedId) == false && string.IsNullOrEmpty(savedTime) == false)
        {
            foreach(ExpeditionData data in _expeditionsList)
            {
                if(data.Id == savedId)
                {
                    _expeditionModel.SelectedExpedition = data;
                    break;
                }
            }

            if(_expeditionModel.SelectedExpedition != null)
            {
                if(DateTime.TryParse(savedTime, out DateTime parsedTime))
                {
                    _expeditionModel.StartTime = parsedTime;

                    _expeditionModel.IsExpeditionStart = true;
                    _expeditionViewModel.IsExpeditionStart = true;

                    StartTimerLoop().Forget();
                }
            }
        }

        _expeditionViewModel.InvokeOnceOnInit();
    }

    public bool TrySelectExpedition(int index)
    {
        if(index < 0 || index >= _expeditionsList.Count)
        {
            return false;
        }

        ExpeditionData targetExpedition = _expeditionsList[index];
        int currentPlayerLevel = SaveManager.Instance.GetPlayerLevel();

        if(currentPlayerLevel < targetExpedition.LimitLevel)
        {
            Debug.Log($"[ExpeditionService] 제한 레벨 부족! 필요레벨: {targetExpedition.LimitLevel}");
            return false;
        }

        _expeditionModel.SelectedExpedition = targetExpedition;
        Debug.Log($"[ExpeditionService] {_expeditionModel.SelectedExpedition.ExpeditionName}을 선택하였습니다.");

        return true;
    }

    public void RequestStartExpedition()
    {
        if(_expeditionModel.SelectedExpedition == null)
        {
            return;
        }

        if(_expeditionModel.IsExpeditionStart == true)
        {
            return;
        }

        _expeditionModel.IsExpeditionStart = true;
        _expeditionViewModel.IsExpeditionStart = true;

        _expeditionModel.IsCompleted = false;
        _expeditionViewModel.IsCompleted = false;

        _expeditionModel.StartTime = DateTime.Now;

        string timeStr = _expeditionModel.StartTime.ToString("O");
        SaveManager.Instance.SaveExpeditionStart(_expeditionModel.SelectedExpedition.Id, timeStr);

        Debug.Log("원정을 보냈습니다!");
        StartTimerLoop().Forget();
    }

    public void RequestClaimReward()
    {
        if(_expeditionModel.IsCompleted == false)
        {
            return;
        }

        if(_expeditionModel.SelectedExpedition == null)
        {
            return;
        }

        long rewardGold = _expeditionModel.SelectedExpedition.RewardGold;
        string[] rewardEquipments = _expeditionModel.SelectedExpedition.RewardEquipments;

        if(rewardGold > 0)
        {
            NetworkManager.Instance.PlayerResourceService.RequestAddGold(rewardGold);
        }

        if(rewardEquipments != null && rewardEquipments.Length > 0)
        {
            EquipmentUtils equipUtils = new EquipmentUtils();
            foreach(string equipId in rewardEquipments)
            {
                equipUtils.AddEquipments(equipId);
            }
        }

        SaveManager.Instance.ClearExpedition();

        _expeditionModel.IsExpeditionStart = false;
        _expeditionViewModel.IsExpeditionStart = false;

        _expeditionModel.IsCompleted = false;
        _expeditionViewModel.IsCompleted = false;

        _expeditionModel.SelectedExpedition = null;
        _expeditionViewModel.RemainTimeString = "00:00:00";

        Debug.Log("원정 보상을 수령하였습니다!");
    }

    private async UniTaskVoid StartTimerLoop()
    {
        // for문을 사용하는 것이 아니라 while을 사용한 이유: 몇 번 반복이 아닌 특정 시간(상태)를 체크해야 하므로
        while(_expeditionModel.IsExpeditionStart == true && _expeditionModel.IsCompleted == false)
        {
            DateTime endTime = _expeditionModel.StartTime.AddHours(_expeditionModel.SelectedExpedition.DurationHours);
            TimeSpan remainTime = endTime - DateTime.Now;

            if(remainTime.TotalSeconds <= 0)
            {
                _expeditionModel.IsCompleted = true;
                _expeditionViewModel.IsCompleted = true;
                _expeditionViewModel.RemainTimeString = "00:00:00";
                break;
            }

            _expeditionViewModel.RemainTimeString = string.Format("{0:D2}:{1:D2}:{2:D2}", remainTime.Hours, remainTime.Minutes, remainTime.Seconds);
            // 1초 쉬지 않으면 무한루프에 들어갈 수 있으므로 1초정도 쉬게 함
            await UniTask.Delay(1000);
        }
    }
}