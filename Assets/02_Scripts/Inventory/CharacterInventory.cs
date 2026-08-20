using System;
using System.Collections.Generic;

public class CharacterInventory
{
    private List<CharacterSaveData> _ownedCharacters = new List<CharacterSaveData>();
    public event Action<List<CharacterSaveData>> OnCharacterChanged;

    public void Init()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
        {
            _ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;

            if (OnCharacterChanged != null)
            {
                OnCharacterChanged(_ownedCharacters);
            }

        }
    }

    public List<CharacterSaveData> GetOwnedCharacters()
    {
        return _ownedCharacters;
    }

}