using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class ExpeditionPartyUi : UiBase
{
    [SerializeField] private Transform _content;
    [SerializeField] private UiButton _buttonClose;
    [SerializeField] private UiButton _buttonCheck;
    [SerializeField] private Transform[] _partySlotTransforms = new Transform[3];
    private List<HunterSlot> _createdSlots = new List<HunterSlot>();
    private HunterSlot[] _partySlots = new HunterSlot[3];
    private const int _firstSlotCount = 30;

    private string[] _originalPartyUids = new string[3];

    private void OnEnable()
    {
        var expParty = SaveManager.Instance.CurrentSaveData.ExpeditionPartyUids;
        for (int i = 0; i < 3; i++)
        {
            _originalPartyUids[i] = expParty[i];
        }

        ReLoadExpeditionPartyUi().Forget();

        _buttonClose.UnBindOnClickButtonEvent(ClosePartyUi);
        _buttonClose.BindOnClickButtonEvent(ClosePartyUi);

        if (_buttonCheck != null)
        {
            _buttonCheck.UnBindOnClickButtonEvent(ConfirmPartySetting);
            _buttonCheck.BindOnClickButtonEvent(ConfirmPartySetting);
        }
    }

    public async UniTaskVoid ReLoadExpeditionPartyUi()
    {
        if (SaveManager.Instance == null || GameDataManager.Instance == null) return;

        var saveData = SaveManager.Instance.CurrentSaveData;
        string[] expPartyUids = saveData.ExpeditionPartyUids;
        string[] mainPartyUids = saveData.CurrentPartyCharacterUids;
        List<CharacterSaveData> ownedCharacters = saveData.OwnedCharacters;

        for (int i = 0; i < 3; i++)
        {
            if (_partySlots[i] == null)
            {
                GameObject pObj = await ResourceManager.Instance.Instantiate("HunterSlot", _partySlotTransforms[i]);
                _partySlots[i] = pObj.GetComponent<HunterSlot>();
            }

            string partyUid = expPartyUids[i];

            if (string.IsNullOrEmpty(partyUid) == false && SaveManager.Instance.CharacterDict.TryGetValue(partyUid, out var charData))
            {
                var data = GameDataManager.Instance.GetData<CharacterData>(charData.BaseId);
                _partySlots[i].SetUp(data, partyUid, OnClickPartySlot, OnLongPressSlot);
            }
            else
            {
                _partySlots[i].SetUp(null, "", null);
            }
        }

        List<CharacterSaveData> waitChars = new List<CharacterSaveData>();
        foreach (var hunter in ownedCharacters)
        {
            bool isExpParty = false;
            for (int i = 0; i < expPartyUids.Length; i++)
            {
                if (expPartyUids[i] == hunter.UniqueId)
                {
                    isExpParty = true;
                    break;
                }
            }

            bool isMainParty = false;
            for (int i = 0; i < mainPartyUids.Length; i++)
            {
                if (mainPartyUids[i] == hunter.UniqueId)
                {
                    isMainParty = true;
                    break;
                }
            }

            if (isExpParty == false && isMainParty == false)
            {
                waitChars.Add(hunter);
            }
        }

        int targetSlotCount = Mathf.Max(_firstSlotCount, waitChars.Count);
        int currentCount = _createdSlots.Count;

        for (int i = currentCount; i < targetSlotCount; i++)
        {
            GameObject slotObj = await ResourceManager.Instance.Instantiate("HunterSlot", _content);
            if (slotObj != null) _createdSlots.Add(slotObj.GetComponent<HunterSlot>());
        }

        for (int i = 0; i < _createdSlots.Count; i++)
        {
            if (i < waitChars.Count)
            {
                var data = GameDataManager.Instance.GetData<CharacterData>(waitChars[i].BaseId);
                _createdSlots[i].SetUp(data, waitChars[i].UniqueId, OnClickSlot, OnLongPressSlot);
                _createdSlots[i].gameObject.SetActive(true);
            }
            else
            {
                _createdSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnClickSlot(string uniqueId)
    {
        ExpeditionPartySetting setting = new ExpeditionPartySetting();
        if (setting.AddHunterToExpedition(uniqueId))
        {
            ReLoadExpeditionPartyUi().Forget();
        }
    }

    private void OnClickPartySlot(string uniqueId)
    {
        ExpeditionPartySetting setting = new ExpeditionPartySetting();
        if (setting.RemoveCharacterFromExpedition(uniqueId))
        {
            ReLoadExpeditionPartyUi().Forget();
        }
    }

    private async UniTaskVoid OpenHunterInfo(string uniqueId)
    {
        if (SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out var charSaveData))
        {
            HunterInfoUi HunterinfoUi = await UiManager.Instance.OpenUi<HunterInfoUi>();
            if (HunterinfoUi != null) HunterinfoUi.SetUp(uniqueId, charSaveData.BaseId).Forget();
        }
    }

    private void OnLongPressSlot(string uniqueId)
    {
        OpenHunterInfo(uniqueId).Forget();
    }

    private void ConfirmPartySetting()
    {
        var expParty = SaveManager.Instance.CurrentSaveData.ExpeditionPartyUids;

        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            if (string.IsNullOrEmpty(expParty[i]) == false) count++;
        }

        if (count < 3)
        {
            Debug.LogWarning("원정대에는 반드시 3명의 헌터를 편성해야 합니다!");
            return;
        }

        bool isChanged = false;
        for (int i = 0; i < 3; i++)
        {
            if (expParty[i] != _originalPartyUids[i])
            {
                isChanged = true;
                break;
            }
        }

        if (isChanged == false)
        {
            return;
        }

        SaveManager.Instance.SaveCurrentData();

        for (int i = 0; i < 3; i++)
        {
            _originalPartyUids[i] = expParty[i];
        }

        ExpeditionPartySetting.OnPartyChanged?.Invoke();
        UiManager.Instance.CloseUi<ExpeditionPartyUi>();
    }

    private void ClosePartyUi()
    {
        var expParty = SaveManager.Instance.CurrentSaveData.ExpeditionPartyUids;
        for (int i = 0; i < 3; i++)
        {
            expParty[i] = _originalPartyUids[i];
        }
        UiManager.Instance.CloseUi<ExpeditionPartyUi>();
    }
}