using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ExpeditionService
{
    private ExpeditionViewModel _expeditionViewModel;
    private List<ExpeditionData> _expeditionsList;

    public ExpeditionViewModel GetExpeditionViewModel()
    {
        if(_expeditionViewModel == null)
        {
            CreateExpeditionViewModel();
        }
        return _expeditionViewModel;
    }

    private void CreateExpeditionViewModel()
    {
        ExpeditionModel expeditionModel = new ExpeditionModel();

        _expeditionViewModel = new ExpeditionViewModel(expeditionModel);
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

        ExpeditionData savedExpedition = null;

        if(string.IsNullOrEmpty(savedId) == false && string.IsNullOrEmpty(savedTime) == false)
        {
            foreach(ExpeditionData data in _expeditionsList)
            {
                if(data.Id == savedId)
                {
                    savedExpedition = data;
                    break;
                }
            }

            if(savedExpedition != null && DateTime.TryParse(savedTime, out DateTime parsedTime))
            {
                _expeditionViewModel.RestoreExpedition(savedExpedition, parsedTime);

                StartTimerLoop().Forget();
            }
        }

        _expeditionViewModel.InvokeOnceOnInit();
    }

    public bool TrySelectExpedition(int index)
    {
        if(index < 0 || index >= _expeditionsList.Count)
        {
            Debug.LogError("[ExpeditionService] 원정대 리스트가 존재하지 않습니다.");
            return false;
        }

        ExpeditionData targetExpedition = _expeditionsList[index];

        if(_expeditionViewModel.SelectExpedition(targetExpedition) == false)
        {
            return false;
        }

        Debug.Log($"[ExpeditionService] {targetExpedition.ExpeditionName}을 선택하였습니다.");
        return true;
    }

    public void RequestStartExpedition()
    {
        if(_expeditionViewModel.SelectedExpedition == null)
        {
            Debug.LogError("[ExpeditionService] 원정대를 선택하지 않았습니다.");
            return;
        }

        if(_expeditionViewModel.IsExpeditionStart == true)
        {
            Debug.LogError("[ExpeditionService] 원정대를 보냈습니다.");
            return;
        }

        int currentPlayerLevel = SaveManager.Instance.GetPlayerLevel();
        int limitLevel = _expeditionViewModel.SelectedExpedition.LimitLevel;
        if (currentPlayerLevel < limitLevel)
        {
            Debug.LogWarning($"[ExpeditionService] 원정 시작 불가: 플레이어 레벨이 부족합니다! (필요 레벨: {limitLevel})");
            return;
        }

        DateTime startTime = DateTime.Now;

        if(_expeditionViewModel.TryStartExpedition(startTime) == false)
        {
            return;
        }

        SaveManager.Instance.SaveExpeditionStart(_expeditionViewModel.SelectedExpedition.Id, startTime.ToString("O"));

        Debug.Log("원정을 보냈습니다!");
        StartTimerLoop().Forget();
    }

    public void RequestClaimReward()
    {
        if(_expeditionViewModel.IsCompleted == false)
        {
            Debug.LogError("[ExpeditionService] 원정대가 완료되지 않았습니다.");
            return;
        }

        ExpeditionData selectedExpedition = _expeditionViewModel.SelectedExpedition;

        if(selectedExpedition == null)
        {
            Debug.LogError("[ExpeditionService] 원정대를 선택하지 않았습니다.");
            return;
        }

        long rewardGold = selectedExpedition.RewardGold;
        string[] rewardEquipments = selectedExpedition.RewardEquipments;

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

        _expeditionViewModel.ResetExpedition();

        Debug.Log("원정 보상을 수령하였습니다!");
    }

    private async UniTaskVoid StartTimerLoop()
    {
        // for문을 사용하는 것이 아니라 while을 사용한 이유: 몇 번 반복이 아닌 특정 시간(상태)를 체크해야 하므로
        while(_expeditionViewModel.IsExpeditionStart == true && _expeditionViewModel.IsCompleted == false)
        {
            ExpeditionData selectedExpedition = _expeditionViewModel.SelectedExpedition;

            if(selectedExpedition == null)
            {
                break;
            }

            DateTime endTime = _expeditionViewModel.StartTime.AddHours(selectedExpedition.DurationHours);
            TimeSpan remainTime = endTime - DateTime.Now;

            if(remainTime.TotalSeconds <= 0)
            {
                _expeditionViewModel.CompletedExpedition();
                break;
            }
            _expeditionViewModel.UpdateRemainTime(remainTime);

            // 1초 쉬지 않으면 무한루프에 들어갈 수 있으므로 1초정도 쉬게 함
            await UniTask.Delay(1000);
        }
    }
}