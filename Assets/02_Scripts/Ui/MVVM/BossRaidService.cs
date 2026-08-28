using System;
using System.Collections.Generic;
using UnityEngine;

public class BossRaidService
{
    private BossRaidViewModel _bossRaidViewModel;
    private List<BossData> _bossList = new List<BossData>();
    private bool _isInitialized;

    public BossRaidViewModel GetBossRaidViewModel()
    {
        Init();
        return _bossRaidViewModel;
    }

    public IReadOnlyList<BossData> GetBossList()
    {
        Init();
        return _bossList;
    }

    private BossData GetSelectedBossData()
    {
        return GetBossRaidViewModel().SelectedBoss;
    }

    public string GetSelectedBossMonsterId()
    {
        BossData selectedBoss = GetSelectedBossData();
        return selectedBoss != null ? selectedBoss.MonsterId : "";
    }

    public void Init()
    {
        if (_bossRaidViewModel == null)
        {
            BossRaidModel model = new BossRaidModel();
            _bossRaidViewModel = new BossRaidViewModel(model);
        }

        if (_isInitialized)
        {
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[BossRaidService] GameDataManager가 없습니다.");
            return;
        }

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[BossRaidService] SaveManager가 초기화되지 않았습니다.");
            return;
        }

        _bossList = GameDataManager.Instance.GetAllData<BossData>();

        if (_bossList == null)
        {
            _bossList = new List<BossData>();
            Debug.LogError("[BossRaidService] BossData 목록을 불러오지 못했습니다.");
            return;
        }

        for (int i = _bossList.Count - 1; i >= 0; i--)
        {
            BossData bossData = _bossList[i];

            if (bossData == null || string.IsNullOrEmpty(bossData.MonsterId))
            {
                Debug.LogError("[BossRaidService] MonsterId가 없는 BossData를 제외합니다.");
                _bossList.RemoveAt(i);
                continue;
            }

            MonsterData monsterData = GameDataManager.Instance.GetData<MonsterData>(bossData.MonsterId);

            if (monsterData == null)
            {
                Debug.LogError($"[BossRaidService] BossData가 참조하는 MonsterData가 없습니다. BossId: {bossData.Id}, MonsterId: {bossData.MonsterId}");
                _bossList.RemoveAt(i);
            }
        }

        _bossList.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));

        ReloadPartyFromSave();

        if (_bossList.Count > 0)
        {
            _bossRaidViewModel.SelectBoss(_bossList[0]);
        }
        else
        {
            Debug.LogWarning("[BossRaidService] 사용할 수 있는 BossData가 없습니다.");
        }

        _bossRaidViewModel.InvokeOnceOnInit();
        _isInitialized = true;
    }

    public void ReloadPartyFromSave()
    {
        if (_bossRaidViewModel == null || _bossRaidViewModel.IsRaidInProgress)
        {
            return;
        }

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            return;
        }

        string[] savedParty = SaveManager.Instance.CurrentSaveData.BossRaidPartyUids;

        string[] validatedParty = new string[BossRaidModel.MaxPartySize];

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            validatedParty[i] = "";

            if (savedParty == null || i >= savedParty.Length)
            {
                continue;
            }

            string uniqueId = savedParty[i];

            if (string.IsNullOrEmpty(uniqueId))
            {
                continue;
            }

            if (SaveManager.Instance.CharacterDict.ContainsKey(uniqueId) == false)
            {
                Debug.LogWarning($"[BossRaidService] 저장된 보스 파티에서 보유하지 않은 헌터를 제외합니다. UID: {uniqueId}");
                continue;
            }

            validatedParty[i] = uniqueId;
        }

        _bossRaidViewModel.ReplaceParty(validatedParty);
    }

    public bool TrySelectBoss(int index)
    {
        Init();

        if (index < 0 || index >= _bossList.Count)
        {
            Debug.LogError("[BossRaidService] 선택한 보스 인덱스가 잘못되었습니다.");
            return false;
        }

        return _bossRaidViewModel.SelectBoss(_bossList[index]);
    }

    public bool TryAddHunter(string uniqueId)
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            Debug.LogWarning("[BossRaidService] 헌터 UID가 비어있습니다.");
            return false;
        }

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[BossRaidService] SaveManager가 초기화되지 않았습니다.");
            return false;
        }

        if (SaveManager.Instance.CharacterDict.ContainsKey(uniqueId) == false)
        {
            Debug.LogWarning($"[BossRaidService] 보유하지 않은 헌터입니다. UID: {uniqueId}");
            return false;
        }

        if (_bossRaidViewModel.TryAddHunter(uniqueId) == false)
        {
            Debug.LogWarning("[BossRaidService] 이미 편성된 헌터이거나 파티가 가득 찼습니다.");
            return false;
        }

        return true;
    }

    public bool TryRemoveHunter(string uniqueId)
    {
        if (_bossRaidViewModel == null)
        {
            return false;
        }

        return _bossRaidViewModel.TryRemoveHunter(uniqueId);
    }

    public void RestoreParty(IReadOnlyList<string> originalPartyUids)
    {
        if (_bossRaidViewModel == null)
        {
            return;
        }

        _bossRaidViewModel.ReplaceParty(originalPartyUids);
    }

    public bool RequestStartBossRaid()
    {
        Init();

        if (_isInitialized == false || _bossRaidViewModel == null)
        {
            Debug.LogError("[BossRaidService] 보스 레이드 서비스가 초기화되지 않았습니다.");
            return false;
        }

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[BossRaidService] SaveManager가 초기화되지 않았습니다.");
            return false;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[BossRaidService] GameDataManager가 없습니다.");
            return false;
        }

        BossData selectedBoss = _bossRaidViewModel.SelectedBoss;

        if (selectedBoss == null)
        {
            Debug.LogWarning("[BossRaidService] 보스를 선택해야 합니다.");
            return false;
        }

        int playerLevel = SaveManager.Instance.GetPlayerLevel();

        if (playerLevel < selectedBoss.LimitLevel)
        {
            Debug.LogWarning($"[BossRaidService] 레벨이 부족합니다. 현재 레벨: {playerLevel}, 필요 레벨: {selectedBoss.LimitLevel}");
            return false;
        }

        if (_bossRaidViewModel.IsPartyComplete == false)
        {
            Debug.LogWarning($"[BossRaidService] 보스 레이드에는 반드시 {BossRaidModel.MaxPartySize}명의 헌터가 필요합니다.");
            return false;
        }

        IReadOnlyList<string> partyUids = _bossRaidViewModel.PartyUids;

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            string uniqueId = partyUids[i];

            if (string.IsNullOrEmpty(uniqueId))
            {
                Debug.LogWarning($"[BossRaidService] {i + 1}번 보스 레이드 파티 슬롯이 비어있습니다.");
                return false;
            }

            if (SaveManager.Instance.CharacterDict.ContainsKey(uniqueId) == false)
            {
                Debug.LogWarning($"[BossRaidService] 보유하지 않은 헌터가 파티에 포함되어 있습니다. UID: {uniqueId}");
                return false;
            }
        }

        MonsterData monsterData = GameDataManager.Instance.GetData<MonsterData>(selectedBoss.MonsterId);

        if (monsterData == null)
        {
            Debug.LogError($"[BossRaidService] 보스 몬스터 데이터를 찾을 수 없습니다. MonsterId: {selectedBoss.MonsterId}");
            return false;
        }

        if (_bossRaidViewModel.TryStartRaid() == false)
        {
            return false;
        }

        SaveManager.Instance.CurrentSaveData.BossRaidPartyUids = _bossRaidViewModel.CopyPartyUids();
        SaveManager.Instance.SaveCurrentData();

        Debug.Log($"[BossRaidService] {monsterData.Name} 레이드를 시작합니다.");
        return true;
    }

    public void RequestCompleteBossRaid(bool isVictory)
    {
        if (_bossRaidViewModel == null || _bossRaidViewModel.IsRaidInProgress == false)
        {
            return;
        }

        BossData selectedBoss = _bossRaidViewModel.SelectedBoss;

        if (isVictory)
        {
            if (selectedBoss == null)
            {
                Debug.LogError("[BossRaidService] 완료할 BossData가 없습니다.");
                return;
            }

            bool hasPlayerResourceReward = selectedBoss.RewardDiamond > 0 || selectedBoss.RewardRank != GuildRank.None;

            if (hasPlayerResourceReward)
            {
                if (NetworkManager.Instance == null || NetworkManager.Instance.PlayerResourceService == null)
                {
                    Debug.LogError("[BossRaidService] PlayerResourceService가 없습니다.");
                    return;
                }
            }
        }

        if (_bossRaidViewModel.CompleteRaid() == false)
        {
            return;
        }

        if (isVictory == false)
        {
            Debug.Log("[BossRaidService] 보스 레이드에 실패하여 보상이 지급되지 않습니다.");
            return;
        }

        if (selectedBoss.RewardDiamond > 0)
        {
            NetworkManager.Instance.PlayerResourceService.RequestAddDiamond(selectedBoss.RewardDiamond);
        }

        Debug.Log($"[BossRaidService] 보스 레이드 승리 보상으로 다이아 {selectedBoss.RewardDiamond}개를 지급했습니다.");

        if (selectedBoss.RewardRank != GuildRank.None)
        {
            bool isRankIncreased = NetworkManager.Instance.PlayerResourceService.RequestIncreasePlayerGuildRank(selectedBoss.RewardRank);

            if (isRankIncreased)
            {
                Debug.Log($"[BossRaidService] 길드 랭크가 {selectedBoss.RewardRank}(으)로 승급했습니다.");
            }

            else
            {
                Debug.Log($"[BossRaidService] 현재 랭크와 승급 순서가 맞지 않아 {selectedBoss.RewardRank} 랭크 보상이 적용되지 않았습니다.");
            }
        }
    }

}