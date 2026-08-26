using System;
using System.Collections.Generic;
using UnityEngine;

public class OffLineRewardService
{
    public TimeSpan OfflineTime { get; private set; }
    public long RewardGold { get; private set; }
    public long RewardExp { get; private set; }
    public long RewardMagicStone { get; private set; }
    public List<EquipmentData> RewardEquipments { get; private set; } = new List<EquipmentData>();

    private const double MaxOfflineHours = 24.0;

    private bool _isCalculated = false;
    private bool _isClaimed = false;

    public bool CalculateOfflineReward()
    {
        if (_isClaimed == true)
        {
            OfflineTime = TimeSpan.Zero;
            RewardGold = 0;
            RewardExp = 0;
            RewardMagicStone = 0;
            RewardEquipments.Clear();
            return false;
        }

        if (_isCalculated == true)
        {
            if (OfflineTime.TotalSeconds <= 0)
            {
                return false;
            }
            return true;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[OfflineRewardService] SaveManager.Instance가 null입니다.");
            return false;
        }

        string logoutTimeStr = SaveManager.Instance.GetLogoutTime();

        if (string.IsNullOrEmpty(logoutTimeStr) == true)
        {
            return false;
        }

        DateTime logoutTime;
        bool isParsed = DateTime.TryParse(logoutTimeStr, out logoutTime);

        if (isParsed == false)
        {
            Debug.LogError("[OfflineRewardService] 로그아웃 시간을 파싱하는데 실패했습니다.");
            return false;
        }

        OfflineTime = DateTime.Now - logoutTime;

        if (OfflineTime.TotalHours > MaxOfflineHours)
        {
            OfflineTime = TimeSpan.FromHours(MaxOfflineHours);
        }

        if (OfflineTime.TotalSeconds <= 0)
        {
            return false;
        }

        int maxStage = SaveManager.Instance.GetMaxClearedStage();

        if (maxStage <= 0)
        {
            maxStage = 1;
        }

        double totalHours = OfflineTime.TotalHours;

        RewardGold = (long)(maxStage * 300 * totalHours);
        RewardExp = (long)(maxStage * 300 * totalHours);
        RewardMagicStone = (long)(maxStage * 10 * totalHours);

        int equipmentDropCount = (int)totalHours;

        if (equipmentDropCount > 0)
        {
            CalculateEquipmentDrops(maxStage, equipmentDropCount);
        }

        _isCalculated = true;
        return true;
    }

    private void CalculateEquipmentDrops(int maxStage, int dropCount)
    {
        RewardEquipments.Clear();

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[OfflineRewardService] GameDataManager.Instance가 null입니다.");
            return;
        }

        List<EquipmentData> allEquips = GameDataManager.Instance.GetAllData<EquipmentData>();

        if (allEquips == null)
        {
            Debug.LogError("[OfflineRewardService] 장비 데이터 리스트가 null입니다.");
            return;
        }

        if (allEquips.Count == 0)
        {
            Debug.LogError("[OfflineRewardService] 장비 데이터 리스트가 비어있습니다.");
            return;
        }

        int t2Prob = 0;
        int t3Prob = 0;

        if (maxStage >= 50)
        {
            t2Prob = 50;
            t3Prob = 30;
        }
        else if (maxStage >= 30)
        {
            t2Prob = 40;
            t3Prob = 10;
        }
        else if (maxStage >= 10)
        {
            t2Prob = 20;
            t3Prob = 0;
        }

        for (int i = 0; i < dropCount; i++)
        {
            int roll = UnityEngine.Random.Range(1, 101);
            EquipmentTier targetTier = EquipmentTier.Normal;

            if (roll <= t3Prob)
            {
                targetTier = EquipmentTier.Epic;
            }
            else if (roll <= t3Prob + t2Prob)
            {
                targetTier = EquipmentTier.Rare;
            }
            else
            {
                targetTier = EquipmentTier.Normal;
            }

            List<EquipmentData> filteredEquips = new List<EquipmentData>();

            for (int j = 0; j < allEquips.Count; j++)
            {
                if (allEquips[j].Tier == targetTier)
                {
                    filteredEquips.Add(allEquips[j]);
                }
            }

            if (filteredEquips.Count > 0)
            {
                int randIndex = UnityEngine.Random.Range(0, filteredEquips.Count);
                RewardEquipments.Add(filteredEquips[randIndex]);
            }
        }
    }

    public void ClaimRewards()
    {
        if (_isClaimed == true)
        {
            return;
        }

        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[OfflineRewardService] NetworkManager.Instance가 null입니다.");
            return;
        }

        if (NetworkManager.Instance.PlayerResourceService == null)
        {
            Debug.LogError("[OfflineRewardService] PlayerResourceService가 null입니다.");
            return;
        }

        PlayerResourceService res = NetworkManager.Instance.PlayerResourceService;

        if (RewardGold > 0)
        {
            res.RequestAddGold(RewardGold);
        }

        if (RewardExp > 0)
        {
            res.RequestAddExp(RewardExp);
        }

        if (RewardMagicStone > 0)
        {
            res.RequestAddMagicStone(RewardMagicStone);
        }

        if (RewardEquipments.Count > 0)
        {
            EquipmentUtils equipUtils = new EquipmentUtils();

            foreach (EquipmentData equip in RewardEquipments)
            {
                if (equip != null)
                {
                    equipUtils.AddEquipments(equip.Id);
                }
            }
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[OfflineRewardService] SaveManager.Instance가 null입니다.");
            return;
        }

        _isClaimed = true;

        SaveManager.Instance.SaveLogoutTime();
        SaveManager.Instance.SaveCurrentData();

        RewardGold = 0;
        RewardExp = 0;
        RewardMagicStone = 0;
        RewardEquipments.Clear();
        OfflineTime = TimeSpan.Zero;
    }
}