using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatusService
{
    private const int MaxPartyCount = 3;

    private CharacterStatusModel[] _characterStatusModels = new CharacterStatusModel[MaxPartyCount];
    private CharacterStatusViewModel[] _characterStatusViewModels = new CharacterStatusViewModel[MaxPartyCount];
    private Character[] _characters = new Character[MaxPartyCount];

    private Action<int, int>[] _hpChangedCallbacks = new Action<int, int>[MaxPartyCount];
    private Action<int, int>[] _skillCostChangedCallbacks = new Action<int, int>[MaxPartyCount];

    public CharacterStatusService()
    {
        
    }
    private void CreateCharacterStatusViewModels()
    {
        for(int i = 0; i<MaxPartyCount; i++)
        {
            CharacterStatusModel characterStatusModel = new CharacterStatusModel();
            CharacterStatusViewModel characterStatusViewModel = new CharacterStatusViewModel(characterStatusModel);

            characterStatusModel.SlotIndex = i;
            _characterStatusModels[i] = characterStatusModel;
            _characterStatusViewModels[i] = characterStatusViewModel;
        }
    }

    public int GetMaxPartyCount()
    {
        return MaxPartyCount;
    }

    public CharacterStatusModel GetCharacterStatusModel(int slotIndex)
    {
        if(slotIndex < 0 || slotIndex >= MaxPartyCount)
        {
            Debug.LogError($"[CharacterStatusService] 파티 슬롯 범위를 벗어났습니다. SlotIndex: {slotIndex}");
            return null;
        }
        return _characterStatusModels[slotIndex];
    }

    public CharacterStatusViewModel GetCharacterStatusViewModel(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxPartyCount)
        {
            Debug.LogError($"[CharacterStatusService] 파티 슬롯 범위를 벗어났습니다. SlotIndex: {slotIndex}");
            return null;
        }

        return _characterStatusViewModels[slotIndex];
    }

    public IReadOnlyList<CharacterStatusViewModel> GetCharacterStatusViewModels()
    {
        return _characterStatusViewModels;
    }

    
}
