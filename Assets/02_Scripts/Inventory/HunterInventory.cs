using System;
using System.Collections.Generic;

public class HunterInventory
{
    private List<CharacterSaveData> _ownedCharacters = new List<CharacterSaveData>();
    public event Action OnInventoryUpdated;

    public void Init()
    {
        ReloadData();
    }

    private void ReloadData()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
        {
            _ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;
        }
    }

    public List<CharacterSaveData> GetOwnedCharacters()
    {
        return _ownedCharacters;
    }

    public void NotifyInventoryChanged()
    {
        ReloadData();
        OnInventoryUpdated?.Invoke();
    }
}