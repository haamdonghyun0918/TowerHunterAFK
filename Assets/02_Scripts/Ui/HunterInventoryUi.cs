using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

public class HunterInventoryUi : UiBase
{
    [SerializeField] private Transform _content;
    private List<HunterSlot> _createdSlots = new List<HunterSlot>();
    private const int _firstSlotCount = 30;

    private void OnEnable()
    {
        ReLoadHunterInventoryUi().Forget();
    }

    public async UniTaskVoid ReLoadHunterInventoryUi()
    {
        if (SaveManager.Instance == null || GameDataManager.Instance == null)
        {
            Debug.LogError("[HunterInventoryUi] SaveManager 또는 GameDataManager가 존재하지 않습니다.");
            return;
        }

        List<CharacterSaveData> ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;

        int targetSlotCount = Mathf.Max(_firstSlotCount, ownedCharacters.Count);

        int currentCount = _createdSlots.Count;
        for (int i = currentCount; i < targetSlotCount; i++)
        {
            GameObject slotObj = await ResourceManager.Instance.Instantiate("HunterSlot", _content);

            if (slotObj != null)
            {
                HunterSlot hunterSlot = slotObj.GetComponent<HunterSlot>();
                if (hunterSlot != null)
                {
                    _createdSlots.Add(hunterSlot);
                }
            }
            else
            {
                Debug.LogError("[HunterInventoryUi]: HunterSlot 프리팹 생성 실패.");
                break;
            }
        }

        for (int i = 0; i < _createdSlots.Count; i++)
        {
            if (i < ownedCharacters.Count)
            {
                string baseId = ownedCharacters[i].BaseId;
                CharacterData charData = GameDataManager.Instance.GetData<CharacterData>(baseId);

                _createdSlots[i].SetUp(charData, i, OnClickSlot);
            }

            else
            {
                _createdSlots[i].SetUp(null, i, OnClickSlot);
            }
        }
    }
    private void OnClickSlot(int index)
    {
        List<CharacterSaveData> ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;

        if (index < ownedCharacters.Count)
        {
            string baseId = ownedCharacters[index].BaseId;
            CharacterData charData = GameDataManager.Instance.GetData<CharacterData>(baseId);

            if (charData != null)
            {
                Debug.Log($"선택된 헌터: {charData.Name}");
            }

        }

        else
        {
            Debug.Log("비어있습니다.");
        }
    }
}