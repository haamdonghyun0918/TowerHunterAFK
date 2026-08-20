using System.Collections.Generic;
using UnityEngine;

public class PartySetting
{
    private const int _maxSlots = 3;

    // 중복된 헌터를 파티에 넣을 경우 사용되는 메서드(시스템적으로)
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

    // 빈자리에 넣는 메서드(실제 플레이어가 버튼을 눌렀을 때 사용할 메서드)
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
                CharacterUtils utils = new CharacterUtils();
                utils.AddCharacters("DevilOrangePlayer");
            }

            if (saveData.OwnedCharacters.Count > 0)
            {
                SetCharacterToParty(0, saveData.OwnedCharacters[0].UniqueId);
            }
        }
    }
}