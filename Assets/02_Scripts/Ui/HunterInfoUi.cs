using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class HunterInfoUi : UiBase
{
    [Header("Profile")]
    [SerializeField] private Image _hunterProfileImage;

    [Header("Stats")]
    [SerializeField] private TMP_Text _textName;
    [SerializeField] private TMP_Text _textHp;
    [SerializeField] private TMP_Text _textAtk;
    [SerializeField] private TMP_Text _textDef;
    [SerializeField] private TMP_Text _textSpd;
    [SerializeField] private TMP_Text _textCost;
    [SerializeField] private TMP_Text _textTier;
    [SerializeField] private TMP_Text _textRank;
    [SerializeField] private TMP_Text _textLevel;

    [Header("Equipment")]
    [SerializeField] private Image _weaponEquipmentImage;
    [SerializeField] private Image _armorEquipmentImage;
    [SerializeField] private Image _accessoryEquipmentImage;
    [SerializeField] private UiButton _buttonWeaponEquipment;
    [SerializeField] private UiButton _buttonArmorEquipment;
    [SerializeField] private UiButton _buttonAccessoryEquipment;

    [Header("Equipment Option")]
    [SerializeField] private GameObject _equipmentOptionPanel;
    [SerializeField] private TMP_Text _textEquipmentOptionName;
    [SerializeField] private UiButton _buttonUnequip;
    [SerializeField] private UiButton _buttonEquipmentEnhance;
    [SerializeField] private UiButton _buttonEquipmentOptionClose;

    [Header("Buttons")]
    [SerializeField] private UiButton _buttonClose;

    [Header("GrowUp")]
    [SerializeField] private TMP_Text _textEnhanceRequirement;
    [SerializeField] private TMP_Text _textLevelUpRequirement;
    [SerializeField] private UiButton _buttonEnhance;
    [SerializeField] private UiButton _buttonLevelUp;

    [SerializeField] private TMP_Text _textEnhanceGold;
    [SerializeField] private TMP_Text _textLevelUpGold;

    private CharacterEquipmentViewModel _equipmentViewModel;
    private CharacterData _characterData;
    private BaseStatData _baseStatData;
    private CharacterSaveData _characterSaveData;

    private Sprite _defaultWeaponSprite;
    private Sprite _defaultArmorSprite;
    private Sprite _defaultAccessorySprite;

    public static event Action OnHunterStateChanged;

    private int _currentLevel = 1;
    private const long _levelUpExp = 2000;

    private List<CharacterSaveData> _pendingMaterials;
    private long _pendingBonusExp;

    private EquipmentSlot _selectedEquipmentSlot = EquipmentSlot.None;

    private void Awake()
    {
        if (_weaponEquipmentImage != null)
        {
            _defaultWeaponSprite = _weaponEquipmentImage.sprite;
        }

        if (_armorEquipmentImage != null)
        {
            _defaultArmorSprite = _armorEquipmentImage.sprite;
        }

        if (_accessoryEquipmentImage != null)
        {
            _defaultAccessorySprite = _accessoryEquipmentImage.sprite;
        }
        if(_equipmentOptionPanel != null)
        {
            _equipmentOptionPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BindButton(_buttonClose, CloseHunterInfoUi);
        BindButton(_buttonWeaponEquipment, OnClickWeaponEquipment);
        BindButton(_buttonArmorEquipment, OnClickArmorEquipment);
        BindButton(_buttonAccessoryEquipment, OnClickAccessoryEquipment);
        BindButton(_buttonEnhance, OnClickEnhance);
        BindButton(_buttonLevelUp, OnClickLevelUp);
    }

    private void OnDisable()
    {
        UnbindViewModel();

        UnbindButton(_buttonClose, CloseHunterInfoUi);
        UnbindButton(_buttonWeaponEquipment, OnClickWeaponEquipment);
        UnbindButton(_buttonArmorEquipment, OnClickArmorEquipment);
        UnbindButton(_buttonAccessoryEquipment, OnClickAccessoryEquipment);
        UnbindButton(_buttonEnhance, OnClickEnhance);
        UnbindButton(_buttonLevelUp, OnClickLevelUp);
        _equipmentViewModel = null;
    }

    public async UniTaskVoid SetUp(string uniqueId, string baseId)
    {
        CharacterData characterData = GameDataManager.Instance.GetData<CharacterData>(baseId);

        if (characterData == null)
        {
            Debug.LogError("[HunterInfoUi] 헌터 데이터를 찾을 수 없습니다.");
            return;
        }

        BaseStatData baseStatData = GameDataManager.Instance.GetData<BaseStatData>(characterData.BaseStatDataId);

        if (baseStatData == null)
        {
            Debug.LogError("[HunterInfoUi] 헌터 기본 능력치를 찾을 수 없습니다.");
            return;
        }

        if (SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out CharacterSaveData characterSaveData) == false)
        {
            Debug.LogError("[HunterInfoUi] 헌터 저장 데이터를 찾을 수 없습니다.");
            return;
        }

        if (NetworkManager.Instance == null || NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[HunterInfoUi] EquipmentService가 없습니다.");
            return;
        }

        _characterData = characterData;
        _baseStatData = baseStatData;
        _characterSaveData = characterSaveData;

        _currentLevel = 1;

        UnbindViewModel();

        _equipmentViewModel = NetworkManager.Instance.EquipmentService.GetCharacterEquipmentViewModel();
        _equipmentViewModel.SetCharacterTarget(uniqueId);

        BindViewModel();
        UpdateHunterInfo();

        await LoadHunterProfileAsync();
    }

    private void BindViewModel()
    {
        if (_equipmentViewModel == null)
        {
            return;
        }

        _equipmentViewModel.PropertyChanged -= OnEquipmentChanged;
        _equipmentViewModel.PropertyChanged += OnEquipmentChanged;
    }

    private void UnbindViewModel()
    {
        if (_equipmentViewModel != null)
        {
            _equipmentViewModel.PropertyChanged -= OnEquipmentChanged;
        }
    }

    private void OnEquipmentChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        UpdateHunterStats();
        UpdateEquipmentIcons();
    }

    private void UpdateHunterInfo()
    {
        SetText(_textName, _characterData.Name);
        SetText(_textCost, _characterData.MaxSkillCost.ToString());
        SetText(_textTier, _characterData.Rarity);

        UpdateUpgradeUI();
        UpdateHunterStats();
        UpdateEquipmentIcons();
    }

    private bool IsCharacterInParty(string uniqueId)
    {
        var saveData = SaveManager.Instance.CurrentSaveData;

        for (int i = 0; i < saveData.CurrentPartyCharacterUids.Length; i++)
        {
            if (saveData.CurrentPartyCharacterUids[i] == uniqueId)
            {
                return true;
            }
        }

        for (int i = 0; i < saveData.ExpeditionPartyUids.Length; i++)
        {
            if (saveData.ExpeditionPartyUids[i] == uniqueId)
            {
                return true;
            }
        }

        return false;
    }

    private int GetMaxLevel(string rarity, int rank)
    {
        int maxLevel = 5;
        switch (rarity)
        {
            case "C": maxLevel = 5 + (rank * 4); break;
            case "B": maxLevel = 10 + (rank * 4); break;
            case "A": maxLevel = 15 + (rank * 5); break;
            case "S": maxLevel = 25 + (rank * 5); break;
        }
        return maxLevel;
    }

    private long GetEnhanceGoldCost(string rarity, int rank)
    {
        if (rarity == "C")
        {
            if (rank == 0)
            {
                return 1000L;
            }
            if (rank == 1)
            {
                return 3000L;
            }
            if (rank == 2)
            {
                return 5000L;
            }
            if (rank == 3)
            {
                return 7000L;
            }
            if (rank == 4)
            {
                return 10000L;
            }
        }

        else if (rarity == "B")
        {
            if (rank == 0)
            {
                return 3000L;
            }
            if (rank == 1)
            {
                return 5000L;
            }
            if (rank == 2)
            {
                return 7000L;
            }
            if (rank == 3)
            {
                return 10000L;
            }
            if (rank == 4)
            {
                return 15000L;
            }
        }

        else if (rarity == "A")
        {
            if (rank == 0)
            {
                return 5000L;
            }
            if (rank == 1)
            {
                return 7000L;
            }
            if (rank == 2)
            {
                return 10000L;
            }
            if (rank == 3)
            {
                return 15000L;
            }
            if (rank == 4)
            {
                return 30000L;
            }
        }

        else if (rarity == "S")
        {
            if (rank == 0)
            {
                return 10000L;
            }
            if (rank == 1)
            {
                return 20000L;
            }
            if (rank == 2)
            {
                return 30000L;
            }
            if (rank == 3)
            {
                return 50000L;
            }
            if (rank == 4)
            {
                return 100000L;
            }
        }

        return 0L;
    }

    private long GetLevelUpGoldCost(string rarity)
    {
        if (rarity == "C")
        {
            return 1500L;
        }

        if (rarity == "B")
        {
            return 3000L;
        }

        if (rarity == "A")
        {
            return 5000L;
        }

        if (rarity == "S")
        {
            return 10000L;
        }

        return 0L;
    }

    private List<CharacterSaveData> GetAvailableMaterials()
    {
        List<CharacterSaveData> materials = new List<CharacterSaveData>();

        foreach (var character in SaveManager.Instance.CurrentSaveData.OwnedCharacters)
        {
            if (character.BaseId == _characterSaveData.BaseId && character.UniqueId != _characterSaveData.UniqueId)
            {
                if (IsCharacterInParty(character.UniqueId) == false)
                {
                    materials.Add(character);
                }
            }
        }

        materials.Sort(CompareMaterialSpec);

        return materials;
    }

    private int CompareMaterialSpec(CharacterSaveData a, CharacterSaveData b)
    {
        int rankComparison = a.Rank.CompareTo(b.Rank);

        if (rankComparison != 0)
        {
            return rankComparison;
        }

        return a.Exp.CompareTo(b.Exp);
    }

    private int GetOwnedDuplicatesCount()
    {
        return GetAvailableMaterials().Count;
    }

    private void UpdateUpgradeUI()
    {
        int maxLevel = GetMaxLevel(_characterData.Rarity, _characterSaveData.Rank);

        _currentLevel = 1 + (int)(_characterSaveData.Exp / _levelUpExp);

        if (_currentLevel > maxLevel)
        {
            _currentLevel = maxLevel;
        }

        SetText(_textLevel, $"{_currentLevel} / {maxLevel}");
        SetText(_textRank, $"{_characterSaveData.Rank} / 5");

        if (_characterSaveData.Rank >= 5)
        {
            SetText(_textEnhanceRequirement, "최대강화");
            SetText(_textEnhanceGold, "MAX");
        }

        else
        {
            int requiredDuplicates = _characterSaveData.Rank + 1;
            int ownedDuplicates = GetOwnedDuplicatesCount();
            SetText(_textEnhanceRequirement, $"{ownedDuplicates} / {requiredDuplicates}");

            long enhanceGold = GetEnhanceGoldCost(_characterData.Rarity, _characterSaveData.Rank);
            SetText(_textEnhanceGold, enhanceGold.ToString("N0"));
        }

        if (_currentLevel >= maxLevel)
        {
            SetText(_textLevelUpRequirement, "최대레벨");
            SetText(_textLevelUpGold, "MAX");
        }

        else
        {
            long ownedExp = SaveManager.Instance.CurrentSaveData.Exp;
            long requiredExp = _levelUpExp;
            SetText(_textLevelUpRequirement, $"{ownedExp} / {requiredExp}");

            long levelUpGold = GetLevelUpGoldCost(_characterData.Rarity);
            SetText(_textLevelUpGold, levelUpGold.ToString("N0"));
        }
    }

    private void OnClickEnhance()
    {
        if (_characterSaveData.Rank >= 5)
        {
            Debug.Log("이미 최대 강화 단계입니다.");
            return;
        }

        int required = _characterSaveData.Rank + 1;
        List<CharacterSaveData> availableMaterials = GetAvailableMaterials();

        if (availableMaterials.Count < required)
        {
            Debug.Log($"강화 재료가 부족합니다. (필요: {required}, 대기 헌터 보유: {availableMaterials.Count})");
            return;
        }

        long requiredGold = GetEnhanceGoldCost(_characterData.Rarity, _characterSaveData.Rank);
        PlayerResourceViewModel resourceVM = NetworkManager.Instance.PlayerResourceService.GetPlayerResourceViewModel();

        if (resourceVM.Gold < requiredGold)
        {
            Debug.LogWarning($"골드가 부족합니다! (필요 골드: {requiredGold})");
            return;
        }


        List<CharacterSaveData> materialsToUse = availableMaterials.GetRange(0, required);

        long totalBonusExp = 0;
        bool hasHighRankMaterial = false;

        for (int i = 0; i < materialsToUse.Count; i++)
        {
            CharacterSaveData mat = materialsToUse[i];
            totalBonusExp += mat.Exp;

            if (mat.Rank > _characterSaveData.Rank)
            {
                hasHighRankMaterial = true;
            }
        }

        if (hasHighRankMaterial)
        {
            ShowWarningPopupAndEnhance(materialsToUse, totalBonusExp).Forget();
        }

        else
        {
            ExecuteEnhance(materialsToUse, totalBonusExp);
        }
    }

    private async UniTaskVoid ShowWarningPopupAndEnhance(List<CharacterSaveData> materialsToUse, long totalBonusExp)
    {
        _pendingMaterials = materialsToUse;
        _pendingBonusExp = totalBonusExp;

        EnhanceWarningUi warningUi = await UiManager.Instance.OpenUi<EnhanceWarningUi>();

        if (warningUi != null)
        {
            warningUi.SetUp(ExecutePendingEnhance);
        }
    }

    private void ExecutePendingEnhance()
    {
        if (_pendingMaterials != null)
        {
            ExecuteEnhance(_pendingMaterials, _pendingBonusExp);
            _pendingMaterials = null;
            _pendingBonusExp = 0;
        }
    }

    private void ExecuteEnhance(List<CharacterSaveData> materialsToUse, long bonusExp)
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.PlayerResourceService == null)
        {
            Debug.LogError("[HunterInfoUi] PlayerResourceService가 없습니다.");
            return;
        }

        long requiredGold = GetEnhanceGoldCost(_characterData.Rarity, _characterSaveData.Rank);
        bool isGoldUsed = NetworkManager.Instance.PlayerResourceService.RequestUseGold(requiredGold);

        if (isGoldUsed == false)
        {
            Debug.LogError("[HunterInfoUi] 골드 차감에 실패했습니다.");
            return;
        }

        for (int i = 0; i < materialsToUse.Count; i++)
        {
            CharacterSaveData mat = materialsToUse[i];
            CharacterSaveData match = null;

            for (int j = 0; j < SaveManager.Instance.CurrentSaveData.OwnedCharacters.Count; j++)
            {
                CharacterSaveData character = SaveManager.Instance.CurrentSaveData.OwnedCharacters[j];

                if (character.UniqueId == mat.UniqueId)
                {
                    match = character;
                    break;
                }
            }

            if (match != null)
            {
                SaveManager.Instance.CurrentSaveData.OwnedCharacters.Remove(match);
            }

            SaveManager.Instance.CharacterDict.Remove(mat.UniqueId);
        }

        _characterSaveData.Rank++;
        _characterSaveData.Exp += bonusExp;

        int newMaxLevel = GetMaxLevel(_characterData.Rarity, _characterSaveData.Rank);
        long maxAllowableExp = (newMaxLevel - 1) * _levelUpExp;

        if (_characterSaveData.Exp > maxAllowableExp)
        {
            long overflowExp = _characterSaveData.Exp - maxAllowableExp;
            _characterSaveData.Exp = maxAllowableExp;
        }

        SaveManager.Instance.SaveCurrentData();

        Debug.Log($"{_characterData.Name}이(가) {_characterSaveData.Rank}강으로 강화되었습니다! (소모 골드: {requiredGold})");

        if (OnHunterStateChanged != null)
        {
            OnHunterStateChanged.Invoke();
        }

        UpdateHunterInfo();
    }

    private void OnClickLevelUp()
    {
        int maxLevel = GetMaxLevel(_characterData.Rarity, _characterSaveData.Rank);

        if (_currentLevel >= maxLevel)
        {
            Debug.Log("최대 레벨이므로 더 이상 경험치를 사용할 수 없습니다.");
            return;
        }

        long ownedExp = SaveManager.Instance.CurrentSaveData.Exp;

        if (ownedExp < _levelUpExp)
        {
            Debug.Log($"경험치가 부족합니다. (보유: {ownedExp} / 필요: {_levelUpExp})");
            return;
        }

        long requiredGold = GetLevelUpGoldCost(_characterData.Rarity);
        PlayerResourceViewModel resourceVM = NetworkManager.Instance.PlayerResourceService.GetPlayerResourceViewModel();

        if (resourceVM.Gold < requiredGold)
        {
            Debug.LogWarning($"골드가 부족합니다! (필요 골드: {requiredGold})");
            return;
        }

        bool isGoldUsed = NetworkManager.Instance.PlayerResourceService.RequestUseGold(requiredGold);

        NetworkManager.Instance.PlayerResourceService.RequestUseExp(_levelUpExp);

        _characterSaveData.Exp += _levelUpExp;
        SaveManager.Instance.SaveCurrentData();

        Debug.Log($"{_characterData.Name} 레벨 업! (현재 레벨: {_currentLevel + 1})");
        OnHunterStateChanged?.Invoke();
        UpdateHunterInfo();
    }

    private void UpdateHunterStats()
    {
        if (_characterData == null || _baseStatData == null)
        {
            return;
        }

        EquipmentStatBonus equipmentBonus = new EquipmentStatBonus();

        if (_equipmentViewModel != null)
        {
            equipmentBonus = _equipmentViewModel.TotalBonus;
        }

        int finalAtk = _baseStatData.BaseAtk + (_characterData.AtkPerLevel * (_currentLevel - 1)) + equipmentBonus.Atk;
        int finalHp = _baseStatData.BaseHp + (_characterData.HpPerLevel * (_currentLevel - 1)) + equipmentBonus.Hp;
        int finalDef = _baseStatData.BaseDef + (_characterData.DefPerLevel * (_currentLevel - 1)) + equipmentBonus.Def;
        int finalSpd = _baseStatData.BaseAtkSpeed + equipmentBonus.AtkSpeed;

        SetText(_textAtk, finalAtk.ToString());
        SetText(_textHp, finalHp.ToString());
        SetText(_textDef, finalDef.ToString());
        SetText(_textSpd, finalSpd.ToString());
    }

    private void UpdateEquipmentIcons()
    {
        if (_equipmentViewModel == null)
        {
            return;
        }

        LoadEquipmentIconAsync(_weaponEquipmentImage, _defaultWeaponSprite, _equipmentViewModel.WeaponIconAddress).Forget();
        LoadEquipmentIconAsync(_armorEquipmentImage, _defaultArmorSprite, _equipmentViewModel.ArmorIconAddress).Forget();
        LoadEquipmentIconAsync(_accessoryEquipmentImage, _defaultAccessorySprite, _equipmentViewModel.AccessoryIconAddress).Forget();
    }

    private async UniTask LoadEquipmentIconAsync(Image targetImage, Sprite defaultSprite, string iconAddress)
    {
        if (targetImage == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(iconAddress))
        {
            targetImage.sprite = defaultSprite;
            return;
        }

        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(iconAddress);

        if (loadedSprite != null)
        {
            targetImage.sprite = loadedSprite;
        }
        else
        {
            targetImage.sprite = defaultSprite;
        }
    }

    private async UniTask LoadHunterProfileAsync()
    {
        if (_hunterProfileImage == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_characterData.IconPath))
        {
            _hunterProfileImage.sprite = null;
            _hunterProfileImage.gameObject.SetActive(false);
            return;
        }

        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(_characterData.IconPath);

        if (loadedSprite != null)
        {
            _hunterProfileImage.sprite = loadedSprite;
            _hunterProfileImage.gameObject.SetActive(true);
        }
    }

    private void OnClickWeaponEquipment()
    {
        OpenEquipmentInventoryAsync(EquipmentSlot.Weapon).Forget();
    }

    private void OnClickArmorEquipment()
    {
        OpenEquipmentInventoryAsync(EquipmentSlot.Armor).Forget();
    }

    private void OnClickAccessoryEquipment()
    {
        OpenEquipmentInventoryAsync(EquipmentSlot.Accessory).Forget();
    }

    private async UniTask OpenEquipmentInventoryAsync(EquipmentSlot slot)
    {
        if (_equipmentViewModel == null)
        {
            return;
        }

        bool canBeginEquip = _equipmentViewModel.RequestBeginEquip(slot);

        if (canBeginEquip == false)
        {
            return;
        }

        await UiManager.Instance.OpenUi<EquipmentInventoryUi>();
    }

    private void BindButton(UiButton button, UnityAction buttonAction)
    {
        if (button == null)
        {
            return;
        }

        button.UnBindOnClickButtonEvent(buttonAction);
        button.BindOnClickButtonEvent(buttonAction);
    }

    private void UnbindButton(UiButton button, UnityAction buttonAction)
    {
        if (button != null)
        {
            button.UnBindOnClickButtonEvent(buttonAction);
        }
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }

    private void CloseHunterInfoUi()
    {
        UiManager.Instance.CloseUi<HunterInfoUi>();
    }
}