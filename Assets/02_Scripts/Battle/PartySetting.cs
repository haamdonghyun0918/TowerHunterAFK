using System.Collections.Generic;
using UnityEngine;

public class PartySetting
{
    private const int _maxSlots = 3;

    public void SetCharacterToParty(int slotIndex, string uniqueId)
    {
        if (slotIndex < 0 || slotIndex >= _maxSlots)
        {
            return;
        }

        string[] currentParty = SaveManager.Instance.CurrentSaveData.CurrentPartyCharacterUids;

        for (int i = 0; i < _maxSlots; i++)
        {
            if (currentParty[i] == uniqueId)
            {
                currentParty[i] = "";
            }
        }

        currentParty[slotIndex] = uniqueId;
        SaveManager.Instance.SaveCurrentData();
    }

    public bool AddCharacterToParty(string uniqueId)
    {
        string[] currentParty = SaveManager.Instance.CurrentSaveData.CurrentPartyCharacterUids;
        
        for (int i = 0; i < _maxSlots; i++)
        {
            if (currentParty[i] == uniqueId)
            {
                Debug.LogWarning("이미 스쿼드에 편성된 헌터입니다.");
                return false;
            }
        }

        for (int i = 0; i < _maxSlots; i++)
        {
            if (string.IsNullOrEmpty(currentParty[i]))
            {
                currentParty[i] = uniqueId;
                SaveManager.Instance.SaveCurrentData();
                return true;
            }
        }

        Debug.LogWarning("스쿼드가 가득 찼습니다 (총 인원수: 3명)");
        return false;
    }

    public bool RemoveCharacterFromParty(string uniqueId)
    {
        string[] currentParty = SaveManager.Instance.CurrentSaveData.CurrentPartyCharacterUids;

        int currentCount = 0;
       
        for(int i = 0; i < _maxSlots; i++)
        {
            if (string.IsNullOrEmpty(currentParty[i]) == false)
            {
                currentCount++;
            }
        }

        if (currentCount <= 1)
        {
            Debug.LogWarning("스쿼드에는 최소 1명의 헌터가 있어야 합니다!");
            return false;
        }

        for (int i = 0; i < _maxSlots; i++)
        {
            if (currentParty[i] == uniqueId)
            {
                currentParty[i] = "";
                SaveManager.Instance.SaveCurrentData();
                return true;
            }
        }

        return false;
    }

    public string[] GetCurrentPartyUids()
    {
        return SaveManager.Instance.CurrentSaveData.CurrentPartyCharacterUids;
    }

    public void TestParty()
    {
        List<CharacterSaveData> ownedChars = SaveManager.Instance.CurrentSaveData.OwnedCharacters;

        if (ownedChars != null && ownedChars.Count > 0)
        {
            string realFirstCharacterUid = ownedChars[0].UniqueId;

            SetCharacterToParty(0, realFirstCharacterUid);
            Debug.Log($"[PartySetting] 테스트 파티 편성 완료! (배치된 진짜 UID: {realFirstCharacterUid})");
        }

        else
        {
            Debug.LogError("[PartySetting] 큰일났습니다! 인벤토리에 캐릭터가 한 명도 없습니다!");
        }
    }
}