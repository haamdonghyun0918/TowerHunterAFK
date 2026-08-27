using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class BossRaidPartyUi : UiBase
{
    [SerializeField] private Transform _content;
    [SerializeField] private UiButton _buttonClose;
    [SerializeField] private UiButton _buttonCheck;
    [SerializeField] private Transform[] _partySlotTransforms = new Transform[BossRaidModel.MaxPartySize];

    private readonly List<HunterSlot> _createdSlots = new List<HunterSlot>();
    private readonly HunterSlot[] _partySlots = new HunterSlot[BossRaidModel.MaxPartySize];
    private readonly string[] _originalPartyUids = new string[BossRaidModel.MaxPartySize];

    private const int _firstSlotCount = 30;

    private BossRaidService _bossRaidService;
    private BossRaidViewModel _bossRaidViewModel;

    private void OnEnable()
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.BossRaidService == null)
        {
            Debug.LogError("[BossRaidPartyUi] BossRaidService가 없습니다.");
            return;
        }

        _bossRaidService = NetworkManager.Instance.BossRaidService;
        _bossRaidViewModel = _bossRaidService.GetBossRaidViewModel();

        string[] currentParty = _bossRaidViewModel.CopyPartyUids();

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            _originalPartyUids[i] = currentParty[i];
        }

        BindButtons();
        ReLoadBossRaidPartyUi().Forget();
    }

    private void BindButtons()
    {
        if (_buttonClose != null)
        {
            _buttonClose.UnBindOnClickButtonEvent(ClosePartyUi);
            _buttonClose.BindOnClickButtonEvent(ClosePartyUi);
        }

        if (_buttonCheck != null)
        {
            _buttonCheck.UnBindOnClickButtonEvent(ConfirmPartySetting);
            _buttonCheck.BindOnClickButtonEvent(ConfirmPartySetting);
        }
    }

    private async UniTaskVoid ReLoadBossRaidPartyUi()
    {
        if (SaveManager.Instance == null || GameDataManager.Instance == null || ResourceManager.Instance == null)
        {
            return;
        }

        if (_partySlotTransforms == null || _partySlotTransforms.Length != BossRaidModel.MaxPartySize)
        {
            Debug.LogError("[BossRaidPartyUi] 파티 슬롯 Transform은 5개가 필요합니다.");
            return;
        }

        IReadOnlyList<string> partyUids = _bossRaidViewModel.PartyUids;

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            if (_partySlotTransforms[i] == null)
            {
                Debug.LogError($"[BossRaidPartyUi] {i + 1}번 파티 슬롯 위치가 연결되지 않았습니다.");
                continue;
            }

            if (_partySlots[i] == null)
            {
                GameObject partySlotObject = await ResourceManager.Instance.Instantiate("HunterSlot", _partySlotTransforms[i]);

                if (partySlotObject != null)
                {
                    _partySlots[i] = partySlotObject.GetComponent<HunterSlot>();
                }
            }

            if (_partySlots[i] == null)
            {
                continue;
            }

            string uniqueId = partyUids[i];

            if (string.IsNullOrEmpty(uniqueId) == false && SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out CharacterSaveData saveData))
            {
                CharacterData characterData = GameDataManager.Instance.GetData<CharacterData>(saveData.BaseId);
                _partySlots[i].SetUp(characterData, uniqueId, OnClickPartySlot, OnLongPressSlot);
            }
            else
            {
                _partySlots[i].SetUp(null, "", null, null);
            }
        }

        List<CharacterSaveData> ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;
        List<CharacterSaveData> waitingCharacters = new List<CharacterSaveData>();

        for (int i = 0; i < ownedCharacters.Count; i++)
        {
            CharacterSaveData hunter = ownedCharacters[i];
            bool isAlreadySelected = false;

            for (int j = 0; j < BossRaidModel.MaxPartySize; j++)
            {
                if (partyUids[j] == hunter.UniqueId)
                {
                    isAlreadySelected = true;
                    break;
                }
            }

            if (isAlreadySelected == false)
            {
                waitingCharacters.Add(hunter);
            }
        }

        int targetSlotCount = Mathf.Max(_firstSlotCount, waitingCharacters.Count);

        for (int i = _createdSlots.Count; i < targetSlotCount; i++)
        {
            GameObject slotObject = await ResourceManager.Instance.Instantiate("HunterSlot", _content);

            if (slotObject != null)
            {
                HunterSlot slot = slotObject.GetComponent<HunterSlot>();

                if (slot != null)
                {
                    _createdSlots.Add(slot);
                }
            }
        }

        for (int i = 0; i < _createdSlots.Count; i++)
        {
            if (i < waitingCharacters.Count)
            {
                CharacterSaveData saveData = waitingCharacters[i];
                CharacterData characterData = GameDataManager.Instance.GetData<CharacterData>(saveData.BaseId);
                _createdSlots[i].SetUp(characterData, saveData.UniqueId, OnClickWaitingSlot, OnLongPressSlot);
                _createdSlots[i].gameObject.SetActive(true);
            }
            else
            {
                _createdSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnClickWaitingSlot(string uniqueId)
    {
        if (_bossRaidService.TryAddHunter(uniqueId))
        {
            ReLoadBossRaidPartyUi().Forget();
        }
    }

    private void OnClickPartySlot(string uniqueId)
    {
        if (_bossRaidService.TryRemoveHunter(uniqueId))
        {
            ReLoadBossRaidPartyUi().Forget();
        }
    }

    private void OnLongPressSlot(string uniqueId)
    {
        OpenHunterInfo(uniqueId).Forget();
    }

    private async UniTaskVoid OpenHunterInfo(string uniqueId)
    {
        if (SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out CharacterSaveData saveData) == false)
        {
            return;
        }

        HunterInfoUi hunterInfoUi = await UiManager.Instance.OpenUi<HunterInfoUi>();

        if (hunterInfoUi != null)
        {
            hunterInfoUi.SetUp(uniqueId, saveData.BaseId).Forget();
        }
    }

    private void ConfirmPartySetting()
    {
        if (_bossRaidViewModel.IsPartyComplete == false)
        {
            Debug.LogWarning("보스 레이드에는 반드시 5명의 헌터를 편성해야 합니다!");
            return;
        }

        UiManager.Instance.CloseUi<BossRaidPartyUi>();
    }

    private void ClosePartyUi()
    {
        _bossRaidService.RestoreParty(_originalPartyUids);
        UiManager.Instance.CloseUi<BossRaidPartyUi>();
    }
}