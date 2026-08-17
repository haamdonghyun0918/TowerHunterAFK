using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory
{
    private List<CharacterSaveData> _ownedCharacters = new List<CharacterSaveData>();
    public event Action<List<CharacterSaveData>> OnCharacterChanged;

    public void Init()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
        {
            _ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;
            // 테스트용 하드코딩
            if (_ownedCharacters.Count == 0)
            {
                GiveTestCharactersAndSetParty();
            }

            if (OnCharacterChanged != null)
            {
                OnCharacterChanged(_ownedCharacters);
            }
        }
    }

    private void GiveTestCharactersAndSetParty()
    {
        Debug.Log("[초기 설정] 헌터 3명 지급 및 기본 스쿼드 편성을 진행합니다.");

        CharacterUtils charUtils = new CharacterUtils();

        charUtils.AddCharacters("character_meteorshower_03");
        charUtils.AddCharacters("character_frontspikesattack_06");
        charUtils.AddCharacters("character_knives_02");

        _ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;
    }

    public List<CharacterSaveData> GetOwnedCharacters()
    {
        return _ownedCharacters;
    }


}