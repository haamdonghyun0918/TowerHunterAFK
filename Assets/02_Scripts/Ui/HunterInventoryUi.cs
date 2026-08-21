using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

public class HunterInventoryUi : UiBase
{
    [SerializeField] private Transform _content;
    [SerializeField] private UiButton _buttonClose;
    [SerializeField] private Transform[] _partySlotTransforms = new Transform[3];
    private List<HunterSlot> _createdSlots = new List<HunterSlot>();
    private HunterSlot[] _partySlots = new HunterSlot[3];
    private const int _firstSlotCount = 30;

    private void OnEnable()
    {
        ReLoadHunterInventoryUi().Forget();
        _buttonClose.BindOnClickButtonEvent(CloseHunterInventory);
    }

    public async UniTaskVoid ReLoadHunterInventoryUi()
    {
        if (SaveManager.Instance == null || GameDataManager.Instance == null)
        {
            Debug.LogError("[HunterInventoryUi] SaveManager 또는 GameDataManager가 존재하지 않습니다.");
            return;
        }

        var saveData = SaveManager.Instance.CurrentSaveData;
        string[] currentPartyUids = saveData.CurrentPartyCharacterUids;
        List<CharacterSaveData> ownedCharacters = saveData.OwnedCharacters;

        for (int i = 0; i < 3; i++)
        {
            if (_partySlots[i] == null)
            {
                GameObject pObj = await ResourceManager.Instance.Instantiate("HunterSlot", _partySlotTransforms[i]);
                _partySlots[i] = pObj.GetComponent<HunterSlot>();
            }

            string partyUid = currentPartyUids[i];

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
            bool isParty = false;
            for (int i = 0; i < 3; i++)
            {
                if (currentPartyUids[i] == hunter.UniqueId)
                {
                    isParty = true;
                }
            }

            if (isParty == false)
            {
                waitChars.Add(hunter);
            }
        }

        int targetSlotCount = Mathf.Max(_firstSlotCount, waitChars.Count);
        int currentCount = _createdSlots.Count;

        for (int i = currentCount; i < targetSlotCount; i++)
        {
            GameObject slotObj = await ResourceManager.Instance.Instantiate("HunterSlot", _content);
            if (slotObj != null)
            {
                _createdSlots.Add(slotObj.GetComponent<HunterSlot>());
            }
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
        PartySetting partySetting = new PartySetting();
        bool isSuccess = partySetting.AddCharacterToParty(uniqueId);
        
        if (isSuccess)
        {
            ReLoadHunterInventoryUi().Forget();
        }

    }

    private void OnClickPartySlot(string uniqueId)
    {
        PartySetting partySetting = new PartySetting();
        bool isSuccess = partySetting.RemoveCharacterFromParty(uniqueId);

        if (isSuccess)
        {
            ReLoadHunterInventoryUi().Forget();
        }
    }

    private async UniTaskVoid OpenHunterInfo(string uniqueId)
    {
        if (SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out var charSaveData))
        {
            HunterInfoUi HunterinfoUi = await UiManager.Instance.OpenUi<HunterInfoUi>();

            if (HunterinfoUi != null)
            {
                HunterinfoUi.SetUp(uniqueId, charSaveData.BaseId).Forget();
            }
        }
    }


    private void OnLongPressSlot(string uniqueId)
    {
        OpenHunterInfo(uniqueId).Forget();
    }

    private void CloseHunterInventory()
    {
        UiManager.Instance.CloseUi<HunterInventoryUi>();
    }
}