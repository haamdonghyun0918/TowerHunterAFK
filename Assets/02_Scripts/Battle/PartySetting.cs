using UnityEngine;

public class PartySetting
{
    private const int _maxSlots = 3;

    public bool AddCharacterToParty(string uniqueId)
    {
        string[] currentParty = SaveManager.Instance.CurrentSaveData.CurrentPartyCharacterUids;

        if (SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out var incomingData) == false)
        {
            return false;
        }

        string incomingBaseId = incomingData.BaseId;
        int emptySlotIndex = -1;

        for (int i = 0; i < _maxSlots; i++)
        {
            string partyUid = currentParty[i];

            if (string.IsNullOrEmpty(partyUid))
            {

                if (emptySlotIndex == -1)
                {
                    emptySlotIndex = i;
                }

                continue;
            }


            if (partyUid == uniqueId)
            {
                Debug.LogWarning("이미 스쿼드에 편성된 헌터입니다.");
                return false;
            }


            if (SaveManager.Instance.CharacterDict.TryGetValue(partyUid, out var partyData))
            {

                if (partyData.BaseId == incomingBaseId)
                {
                    currentParty[i] = uniqueId;
                    SaveManager.Instance.SaveCurrentData();
                    Debug.Log($"[PartySetting] 이미 편성된 같은 종류의 헌터({incomingBaseId})를 교체했습니다.");
                    return true;
                }
            }
        }

        if (emptySlotIndex != -1)
        {
            currentParty[emptySlotIndex] = uniqueId;
            SaveManager.Instance.SaveCurrentData();
            return true;
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

    public void CreateHunterParty()
    {
        var saveData = SaveManager.Instance.CurrentSaveData;
        string[] currentParty = saveData.CurrentPartyCharacterUids;

        bool isPartyEmpty = true;
        
        for (int i = 0; i < _maxSlots; i++)
        {
            if (string.IsNullOrEmpty(currentParty[i]) == false)
            {
                isPartyEmpty = false;
                break;
            }
        }

        // 처음 게임 시작할 때 줄 기본 캐릭터로 나중에 변경해줘야 함
        if (isPartyEmpty)
        {
            if (saveData.OwnedCharacters.Count == 0)
            {
                HunterUtil utils = new HunterUtil();
                utils.AddCharacters("DevilOrangePlayer");
            }

            if (saveData.OwnedCharacters.Count > 0)
            {
                AddCharacterToParty(saveData.OwnedCharacters[0].UniqueId);
            }
        }
    }
}