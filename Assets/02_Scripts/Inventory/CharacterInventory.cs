using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    public static CharacterInventory Instance { get; private set; }
    private List<CharacterSaveData> _ownedCharacters = new List<CharacterSaveData>();
    public event Action<List<CharacterSaveData>> OnCharacterChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public UniTask Init()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
        {
            _ownedCharacters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;
            OnCharacterChanged?.Invoke(_ownedCharacters);
        }

        Debug.Log("CharacterInventory 호출");
        return UniTask.CompletedTask;
    }

    public List<CharacterSaveData> GetOwnedCharacters()
    {
        return _ownedCharacters;
    }
}