using System;
using System.Collections.Generic;
using UnityEngine;

public struct EquipmentStatBonus
{
    public int Atk;
    public int Hp;
    public int AtkSpeed;
    public int Def;

    public void Add(EquipmentModel equipmentModel)
    {
        if(equipmentModel == null)
        {
            return;
        }
        Atk = Atk + equipmentModel.GetEquipmentTotalAtk();
        Hp = Hp + equipmentModel.GetEquipmentTotalHp();
        AtkSpeed = AtkSpeed + equipmentModel.GetEquipmentTotalAtkSpeed();
        Def = Def + equipmentModel.GetEquipmentTotalDef();
    }
}
public class EquipmentService
{
    private readonly Dictionary<string, EquipmentModel> _equipmentModelDict = new Dictionary<string, EquipmentModel>();

    private EquipmentInventoryViewModel _equipmentInventoryViewModel;
    private EquipmentDetailViewModel _equipmentDetailViewModel;
    private EquipmentEnhanceViewModel _equipmentEnhanceViewModel;
    private EquipmentDisassembleViewModel _equipmentDisassembleViewModel;
    private CharacterEquipmentViewModel _characterEquipmentViewModel;

    public event Action<string> CharacterEquipmentChanged;

    public string EnhanceCostCurrencyName
    {
        get
        {
            return "마석";
        }
    }

    public string DisassembleRewardCurrencyName
    {
        get
        {
            return "마석";
        }
    }

    public EquipmentInventoryViewModel GetEquipmentInventoryViewModel()
    {
        if (_equipmentInventoryViewModel == null)
        {
            _equipmentInventoryViewModel = new EquipmentInventoryViewModel(this);
        }
        return _equipmentInventoryViewModel;
    }

    public EquipmentDetailViewModel GetEquipmentDetailViewModel()
    {
        if (_equipmentDetailViewModel == null)
        {
            _equipmentDetailViewModel = new EquipmentDetailViewModel(this);
        }

        return _equipmentDetailViewModel;
    }

    public EquipmentEnhanceViewModel GetEquipmentEnhanceViewModel()
    {
        if (_equipmentEnhanceViewModel == null)
        {
            _equipmentEnhanceViewModel = new EquipmentEnhanceViewModel(this);
        }

        return _equipmentEnhanceViewModel;
    }

    public EquipmentDisassembleViewModel GetEquipmentDisassembleViewModel()
    {
        if (_equipmentDisassembleViewModel == null)
        {
            _equipmentDisassembleViewModel = new EquipmentDisassembleViewModel(this);
        }

        return _equipmentDisassembleViewModel;
    }

    public CharacterEquipmentViewModel GetCharacterEquipmentViewModel()
    {
        if (_characterEquipmentViewModel == null)
        {
            _characterEquipmentViewModel = new CharacterEquipmentViewModel(this);
        }

        return _characterEquipmentViewModel;
    }

    public bool TryGetEquipmentModel(string uniqueId, out EquipmentModel equipmentModel)
    {
        equipmentModel = null;
        if(string.IsNullOrEmpty(uniqueId))
        {
            Debug.LogError("[EquipmentService]: 장비 UniqueId가 비어있습니다.");
            return false;
        }
        if(_equipmentModelDict.TryGetValue(uniqueId, out equipmentModel))
        {
            return true;
        }
        if(SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[EquipmentService] SaveManager가 초기화되지 않았습니다.");
            return false;
        }
        if(SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData equipmentSaveData) == false )
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return false;
        }

        if(GameDataManager.Instance == null)
        {
            Debug.LogError("[EquipmentService] GameDataManager가 없습니다.");
            return false;
        }

        EquipmentData baseData = GameDataManager.Instance.GetData<EquipmentData>(equipmentSaveData.BaseId);

        if(baseData == null )
        {
            Debug.LogError($"[EquipmentService] 장비 원본 데이터를 찾을 수 없습니다. BaseId: {equipmentSaveData.BaseId}");
            return false;
        }

        equipmentModel = new EquipmentModel(equipmentSaveData, baseData);
        _equipmentModelDict[uniqueId] = equipmentModel;

        return true;
    }

    public IReadOnlyList<EquipmentModel> GetOwnedEquipmentModels()
    {
        List<EquipmentModel> equipmentModels = new List<EquipmentModel>();

        if(SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            return equipmentModels;
        }

        List<EquipmentSaveData> ownedEquipments = SaveManager.Instance.CurrentSaveData.OwnedEquipments;

        if(ownedEquipments == null)
        {
            return equipmentModels;
        }

        for(int i = 0; i < ownedEquipments.Count; i++)
        {
            EquipmentSaveData saveData = ownedEquipments[i];
            if(saveData == null)
            {
                continue;
            }
            if(TryGetEquipmentModel(saveData.UniqueId, out EquipmentModel equipmentModel))
            {
                equipmentModels.Add(equipmentModel);
            }
        }
        return equipmentModels;
    }

    public void RefreshEquipmentInventory()
    {
        GetEquipmentInventoryViewModel().Refresh();
    }
    
    public bool TrySetDetailTarget(string uniqueId)
    {
        if (TryGetEquipmentModel(uniqueId, out EquipmentModel equipmentModel) == false)
        {
            return false;
        }

        GetEquipmentDetailViewModel().SetTarget(equipmentModel);
        return true;
    }

    public bool TrySetEnhanceTarget(string uniqueId)
    {
        if(TryGetEquipmentModel(uniqueId, out EquipmentModel equipmentModel)==false)
        {
            return false;
        }
        GetEquipmentEnhanceViewModel().SetTarget(equipmentModel);
        return true;
    }

    public void SetEnhanceTarget(string uniqueId)
    {
        TrySetEnhanceTarget(uniqueId);
    }
    
    public bool TrySetDisassembleTarget(string uniqueId)
    {
        if( TryGetEquipmentModel(uniqueId,out EquipmentModel equipmentModel)==false)
        {
            return false;
        }
        GetEquipmentDisassembleViewModel().SetTarget(equipmentModel);
        return true;
    }

    public void SetDisassembleTarget(string uniqueId)
    {
        TrySetDisassembleTarget(uniqueId);
    }


    public long GetEnhanceCost(EquipmentModel equipmentModel)
    {
        if(equipmentModel == null)
        {
            return 0;
        }
        //Todo DataDriven JU
        //추후 강화 레벨별 마석 데이터로 교체
        return (equipmentModel.EnhanceLevel + 1) * 10L;
    }

    public bool RequestEnhance(string uniqueId)
    {
        if(TryGetEquipmentModel(uniqueId, out EquipmentModel equipmentModel) == false)
        {
            return false;
        }

        PlayerResourceService resourceService = GetPlayerResourceService();

        if(resourceService == null)
        {
            return false;
        }
        long enhanceCost = GetEnhanceCost(equipmentModel);

        if(enhanceCost <= 0)
        {
            Debug.LogError("[EquipmentService] 강화 비용이 올바르지 않습니다.");
            return false;
        }

        bool isMagicStoneUsed = resourceService.RequestUseMagicStone(enhanceCost);

        if(isMagicStoneUsed == false)
        {
            Debug.LogWarning("[EquipmentService] 마석이 부족합니다.");
            return false;
        }

        equipmentModel.AddEquipmentEnhanceLevel();

        SaveManager.Instance.SaveCurrentData();

        NotifyEquipmentChanged(uniqueId);

        Debug.Log($"[EquipmentService] 강화 성공: {uniqueId}, 사용 마석: {enhanceCost}");

        return true;
    }

    private PlayerResourceService GetPlayerResourceService()
    {
        if(NetworkManager.Instance == null || NetworkManager.Instance.PlayerResourceService == null)
        {
            Debug.LogError("[EquipmentService] PlayerResourceService가 없습니다.");
            return null;
        }
        
        return NetworkManager.Instance.PlayerResourceService;
    }

    public long GetDisassembleReward(EquipmentModel equipmentModel)
    {
        if(equipmentModel == null)
        {
            return 0;
        }

        //Todo DataDriven JU
        // 임시 공식. 추후 변경 고려

        return Math.Max(1L, equipmentModel.Price / 10L);
    }

    public bool RequestDisassemble(string uniqueId)
    {
        if(TryGetEquipmentModel(uniqueId, out EquipmentModel equipmentModel) == false)
        {
            return false;
        }

        if(IsEquipmentEquipped(uniqueId))
        {
            Debug.LogWarning("[EquipmentService]: 장착 중인 장비는 분해할 수 없습니다.");

            return false;
        }
        PlayerResourceService resourceService = GetPlayerResourceService();
        if(resourceService == null)
        {
            return false;
        }

        long reward = GetDisassembleReward(equipmentModel);

        if(SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData equipmentSaveData)==false)
        {
            return false;
        }

        bool isRemove = SaveManager.Instance.CurrentSaveData.OwnedEquipments.Remove(equipmentSaveData);

        if(isRemove == false)
        {
            return false;
        }

        SaveManager.Instance.EquipmentDict.Remove(uniqueId);
        _equipmentModelDict.Remove(uniqueId);

        if(_equipmentDetailViewModel != null && _equipmentDetailViewModel.TargetEquipmentUniqueId == uniqueId)
        {
            _equipmentDetailViewModel.ClearTarget();
        }

        if(_equipmentEnhanceViewModel != null && _equipmentEnhanceViewModel.TargetEquipmentUniqueId == uniqueId)
        {
            _equipmentEnhanceViewModel.ClearTarget();
        }
        if (_equipmentDisassembleViewModel != null && _equipmentDisassembleViewModel.TargetEquipmentUniqueId == uniqueId)
        {
            _equipmentDisassembleViewModel.ClearTarget();
        }

        resourceService.RequestAddMagicStone(reward);
        RefreshEquipmentInventory();

        Debug.Log($"[EquipmentService] 분해 성공. UID: {uniqueId}, 획득 마석: {reward}");

        return true;
    }
    public bool BeginEquipSelection(string characterUniqueId, EquipmentSlot slot)
    {
        if (slot == EquipmentSlot.None || TryGetCharacterData(characterUniqueId, out CharacterSaveData _, out CharacterData _) == false)
        {
            return false;
        }

        GetEquipmentInventoryViewModel().SetEquipMode(characterUniqueId, slot);
        return true;
    }

    public void CancelEquipSelection()
    {
        if (_equipmentInventoryViewModel != null)
        {
            _equipmentInventoryViewModel.SetBrowseMode();
        }
    }

    public bool CanEquip(string characterUniqueId, string equipmentUniqueId, out string failureReason)
    {
        failureReason = "";

        if (TryGetCharacterData(characterUniqueId, out CharacterSaveData characterSaveData, out CharacterData characterData) == false)
        {
            failureReason = "캐릭터 정보를 찾을 수 없습니다.";
            return false;
        }

        if (TryGetEquipmentModel(equipmentUniqueId, out EquipmentModel equipmentModel) == false)
        {
            failureReason = "장비 정보를 찾을 수 없습니다.";
            return false;
        }

        if (equipmentModel.Slot == EquipmentSlot.None)
        {
            failureReason = "알 수 없는 장비 Position입니다: " + equipmentModel.Position;
            return false;
        }

        if (equipmentModel.Slot == EquipmentSlot.Weapon)
        {
            if (equipmentModel.Range == EquipmentRange.None)
            {
                failureReason = "무기 Range가 설정되지 않았습니다.";
                return false;
            }

            if (characterData.WeaponRange == EquipmentRange.None)
            {
                failureReason = "캐릭터 WeaponRange가 설정되지 않았습니다.";
                return false;
            }

            if (characterData.WeaponRange != equipmentModel.Range)
            {
                failureReason = $"무기 사거리가 맞지 않습니다. 캐릭터: {characterData.WeaponRange}, 무기: {equipmentModel.Range}";
                return false;
            }
        }

        if (TryGetEquippedCharacterUniqueId(equipmentUniqueId, out string ownerCharacterUniqueId) && ownerCharacterUniqueId != characterSaveData.UniqueId)
        {
            failureReason = "다른 캐릭터가 장착 중인 장비입니다.";
            return false;
        }

        return true;
    }

    public bool RequestEquip(string characterUniqueId, string equipmentUniqueId)
    {
        if (CanEquip(characterUniqueId, equipmentUniqueId, out string failureReason) == false)
        {
            Debug.LogWarning("[EquipmentService] 장착 실패: " + failureReason);
            return false;
        }

        CharacterSaveData characterSaveData = SaveManager.Instance.CharacterDict[characterUniqueId];
        EquipmentModel equipmentModel = _equipmentModelDict[equipmentUniqueId];
        string previousEquipmentUniqueId = GetEquippedEquipmentUniqueId(characterSaveData, equipmentModel.Slot);

        if (previousEquipmentUniqueId == equipmentUniqueId)
        {
            return true;
        }

        SetEquippedEquipmentUniqueId(characterSaveData, equipmentModel.Slot, equipmentUniqueId);

        SaveManager.Instance.SaveCurrentData();

        NotifyCharacterEquipmentChanged(characterUniqueId);

        RefreshEquipmentInventory();

        Debug.Log($"[EquipmentService] 장착 성공. 캐릭터: {characterUniqueId}, 장비: {equipmentUniqueId}");
        return true;
    }

    public bool RequestUnequip(string characterUniqueId, EquipmentSlot slot)
    {
        if (slot == EquipmentSlot.None || TryGetCharacterData(characterUniqueId, out CharacterSaveData characterSaveData, out CharacterData _) == false)
        {
            return false;
        }

        string equipmentUniqueId = GetEquippedEquipmentUniqueId(characterSaveData, slot);

        if (string.IsNullOrEmpty(equipmentUniqueId))
        {
            return true;
        }

        SetEquippedEquipmentUniqueId(characterSaveData, slot, "");

        SaveManager.Instance.SaveCurrentData();
        NotifyCharacterEquipmentChanged(characterUniqueId);
        RefreshEquipmentInventory();

        return true;
    }

    public bool IsEquipmentEquipped(string equipmentUniqueId)
    {
        return TryGetEquippedCharacterUniqueId(equipmentUniqueId, out string _);
    }

    public bool IsEquipmentEquippedByCharacter(string characterUniqueId, string equipmentUniqueId)
    {
        return TryGetEquippedCharacterUniqueId(equipmentUniqueId, out string ownerCharacterUniqueId) && ownerCharacterUniqueId == characterUniqueId;
    }

    public bool TryGetEquippedCharacterUniqueId(string equipmentUniqueId, out string characterUniqueId)
    {
        characterUniqueId = "";

        if (string.IsNullOrEmpty(equipmentUniqueId) || SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            return false;
        }

        List<CharacterSaveData> characters = SaveManager.Instance.CurrentSaveData.OwnedCharacters;

        if (characters == null)
        {
            return false;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterSaveData character = characters[i];

            if (character != null && (character.EquippedWeaponUid == equipmentUniqueId || character.EquippedArmorUid == equipmentUniqueId || character.EquippedAccessoryUid == equipmentUniqueId))
            {
                characterUniqueId = character.UniqueId;
                return true;
            }
        }

        return false;
    }

    public EquipmentModel GetEquippedEquipmentModel(string characterUniqueId, EquipmentSlot slot)
    {
        if (TryGetCharacterData(characterUniqueId, out CharacterSaveData characterSaveData, out CharacterData _) == false)
        {
            return null;
        }

        string equipmentUniqueId = GetEquippedEquipmentUniqueId(characterSaveData, slot);

        return TryGetEquipmentModel(equipmentUniqueId, out EquipmentModel equipmentModel) ? equipmentModel : null;
    }

    public EquipmentStatBonus GetCharacterEquipmentStatBonus(string characterUniqueId)
    {
        EquipmentStatBonus result = new EquipmentStatBonus();

        if (TryGetCharacterData(characterUniqueId, out CharacterSaveData characterSaveData, out CharacterData _) == false)
        {
            return result;
        }

        AddEquipmentStat(characterSaveData.EquippedWeaponUid, ref result);
        AddEquipmentStat(characterSaveData.EquippedArmorUid, ref result);
        AddEquipmentStat(characterSaveData.EquippedAccessoryUid, ref result);

        return result;
    }

    public bool TryGetCharacterData(string characterUniqueId, out CharacterSaveData characterSaveData, out CharacterData characterData)
    {
        characterSaveData = null;
        characterData = null;

        if (string.IsNullOrEmpty(characterUniqueId) || SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null || GameDataManager.Instance == null)
        {
            return false;
        }

        if (SaveManager.Instance.CharacterDict.TryGetValue(characterUniqueId, out characterSaveData) == false)
        {
            return false;
        }

        characterData = GameDataManager.Instance.GetData<CharacterData>(characterSaveData.BaseId);
        return characterData != null;
    }

    private void AddEquipmentStat(string equipmentUniqueId, ref EquipmentStatBonus result)
    {
        if (TryGetEquipmentModel(equipmentUniqueId, out EquipmentModel equipmentModel))
        {
            result.Add(equipmentModel);
        }
    }

    private string GetEquippedEquipmentUniqueId(CharacterSaveData characterSaveData, EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                return characterSaveData.EquippedWeaponUid;

            case EquipmentSlot.Armor:
                return characterSaveData.EquippedArmorUid;

            case EquipmentSlot.Accessory:
                return characterSaveData.EquippedAccessoryUid;

            default:
                return "";
        }
    }

    private void SetEquippedEquipmentUniqueId(CharacterSaveData characterSaveData, EquipmentSlot slot, string equipmentUniqueId)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                characterSaveData.EquippedWeaponUid = equipmentUniqueId;
                break;

            case EquipmentSlot.Armor:
                characterSaveData.EquippedArmorUid = equipmentUniqueId;
                break;

            case EquipmentSlot.Accessory:
                characterSaveData.EquippedAccessoryUid = equipmentUniqueId;
                break;
        }
    }

    private void NotifyEquipmentChanged(string uniqueId)
    {
        if (_equipmentInventoryViewModel != null)
        {
            _equipmentInventoryViewModel.Refresh();
        }

        if (_equipmentDetailViewModel != null)
        {
            _equipmentDetailViewModel.NotifyEquipmentChanged(uniqueId);
        }

        if (_equipmentEnhanceViewModel != null)
        {
            _equipmentEnhanceViewModel.NotifyEquipmentChanged(uniqueId);
        }

        if (_equipmentDisassembleViewModel != null)
        {
            _equipmentDisassembleViewModel.NotifyEquipmentChanged(uniqueId);
        }

        if (_characterEquipmentViewModel != null)
        {
            _characterEquipmentViewModel.NotifyAllChanged();
        }

        if (TryGetEquippedCharacterUniqueId(uniqueId, out string ownerCharacterUniqueId))
        {
            CharacterEquipmentChanged?.Invoke(ownerCharacterUniqueId);
        }
    }
    private void NotifyCharacterEquipmentChanged(string characterUniqueId)
    {
        CharacterEquipmentChanged?.Invoke(characterUniqueId);

        if (_characterEquipmentViewModel != null)
        {
            _characterEquipmentViewModel.NotifyCharacterEquipmentChanged(characterUniqueId);
        }
    }

}