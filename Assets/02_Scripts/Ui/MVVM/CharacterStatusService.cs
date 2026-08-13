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
        CreateCharacterStatusViewModels();
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

    public void SetParty(PlayerPartyController playerParty)
    {
        if (playerParty == null)
        {
            Debug.LogError("[CharacterStatusService] PlayerPartyController가 없습니다.");
            ClearParty();
            return;
        }

        for(int i = 0; i < MaxPartyCount; i++)
        {
            Character character = playerParty.GetHunter(i);
            SetCharacter(i, character);
        }
    }

    public void ClearParty()
    {
        for(int i = 0; i < MaxPartyCount;i++)
        {
            RemoveCharacter(i);
        }
    }


    public void RemoveCharacter(int slotIndex)
    {
        if(slotIndex < 0 || slotIndex >= MaxPartyCount)
        {
            Debug.LogError($"[CharacterStatusService] 파티 슬롯 범위를 벗어났습니다. SlotIndex: {slotIndex}");
            return;
        }
        UnbindCharacter(slotIndex);
        ResetCharacterStatus(slotIndex);
    }

    private void UnbindCharacter(int slotIndex)
    {
        Character character = _characters[slotIndex];

        if(character != null)
        {
            if (_hpChangedCallbacks[slotIndex] != null)
            {
                character.UnbindOnStatChangedEvent(_hpChangedCallbacks[slotIndex]);
            }

            if (_skillCostChangedCallbacks[slotIndex] != null)
            {
                character.UnbindOnSkillCostChangedEvent(_skillCostChangedCallbacks[slotIndex]);
            }
        }

        _characters[slotIndex] = null;
        _hpChangedCallbacks[slotIndex] = null;
        _skillCostChangedCallbacks[slotIndex] = null;
    }

    private void ResetCharacterStatus(int slotIndex)
    {
        CharacterStatusViewModel characterStatusViewModel = _characterStatusViewModels[slotIndex];

        characterStatusViewModel.CharacterId = "";
        characterStatusViewModel.CurrentHp = 0;
        characterStatusViewModel.MaxHp = 0;
        characterStatusViewModel.CurrentSkillCost = 0;
        characterStatusViewModel.MaxSkillCost = 0;
        characterStatusViewModel.IsActive = false;
        characterStatusViewModel.IsDead = false;
        characterStatusViewModel.InvokeOnceOnInit();
    }

    public void SetCharacter(int slotIndex, Character character)
    {
        if (slotIndex < 0 || slotIndex >= MaxPartyCount)
        {
            Debug.LogError($"[CharacterStatusService] 파티 슬롯 범위를 벗어났습니다. SlotIndex: {slotIndex}");
            return;
        }

        if (_characters[slotIndex] == character)
        {
            UpdateCharacterStatus(slotIndex);
            return;
        }

        UnbindCharacter(slotIndex);

        _characters[slotIndex] = character;

        if (character == null)
        {
            ResetCharacterStatus(slotIndex);
            return;
        }

        _hpChangedCallbacks[slotIndex] = (currentHp, maxHp) => HandleHpChanged(slotIndex, currentHp, maxHp);
        _skillCostChangedCallbacks[slotIndex] = (currentSkillCost, maxSkillCost) => HandleSkillCostChanged(slotIndex, currentSkillCost, maxSkillCost);

        character.BindOnStatChangedEvent(_hpChangedCallbacks[slotIndex]);
        character.BindOnSkillCostChangedEvent(_skillCostChangedCallbacks[slotIndex]);

        UpdateCharacterStatus(slotIndex);

    }

    private void HandleHpChanged(int slotIndex, int currentHp, int maxHp)
    {
        CharacterStatusViewModel characterStatusViewModel = _characterStatusViewModels[slotIndex];

        characterStatusViewModel.CurrentHp = currentHp;
        characterStatusViewModel.MaxHp = maxHp;
        characterStatusViewModel.IsDead = currentHp <= 0;
    }

    private void HandleSkillCostChanged(int slotIndex, int currentSkillCost, int maxSkillCost)
    {
        CharacterStatusViewModel characterStatusViewModel = _characterStatusViewModels[slotIndex];

        characterStatusViewModel.CurrentSkillCost = currentSkillCost;
        characterStatusViewModel.MaxSkillCost = maxSkillCost;
    }

    private void UpdateCharacterStatus(int slotIndex)
    {
        Character character = _characters[slotIndex];

        if(character == null)
        {
            ResetCharacterStatus(slotIndex );
            return;
        }

        CharacterStatusViewModel characterStatusViewModel = _characterStatusViewModels[slotIndex];

        characterStatusViewModel.CharacterId = character.GetCharacterId();
        characterStatusViewModel.CurrentHp = character.GetCurrentHp();
        characterStatusViewModel.MaxHp = character.GetMaxHp();
        characterStatusViewModel.CurrentSkillCost = character.GetCurrentSkillCost();
        characterStatusViewModel.MaxSkillCost = character.GetMaxSkillCost();
        characterStatusViewModel.IsActive = true;
        characterStatusViewModel.IsDead = character._isDead;
        characterStatusViewModel.InvokeOnceOnInit();
    }
}
