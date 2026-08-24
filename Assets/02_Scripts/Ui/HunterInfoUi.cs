using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    [Header("Buttons")]
    [SerializeField] private UiButton _buttonClose;

    [Header("GrowUp")]
    [SerializeField] private TMP_Text _textEnhanceRequirement;
    [SerializeField] private TMP_Text _textLevelUpRequirement;
    [SerializeField] private UiButton _buttonEnhance;
    [SerializeField] private UiButton _buttonLevelUp;

    private CharacterEquipmentViewModel _equipmentViewModel;
    private CharacterData _characterData;
    private BaseStatData _baseStatData;
    private CharacterSaveData _characterSaveData;

    private Sprite _defaultWeaponSprite;
    private Sprite _defaultArmorSprite;
    private Sprite _defaultAccessorySprite;

    private int _currentLevel = 1;
    private const long _levelUpExp = 2000;
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

    private int GetOwnedDuplicatesCount()
    {
        int count = 0;
        foreach (var character in SaveManager.Instance.CurrentSaveData.OwnedCharacters)
        {
            if (character.BaseId == _characterSaveData.BaseId && character.UniqueId != _characterSaveData.UniqueId)
            {
                count++;
            }
        }
        return count;
    }

    private void UpdateUpgradeUI()
    {
        int maxLevel = GetMaxLevel(_characterData.Rarity, _characterSaveData.Rank);

        _currentLevel = 1 + (int)(_characterSaveData.Exp / _levelUpExp);
        if (_currentLevel > maxLevel) _currentLevel = maxLevel;

        SetText(_textLevel, $"{_currentLevel} / {maxLevel}");
        SetText(_textRank, $"{_characterSaveData.Rank} / 5");

        if (_characterSaveData.Rank >= 5)
        {
            SetText(_textEnhanceRequirement, "최대강화");
        }

        else
        {
            int requiredDuplicates = _characterSaveData.Rank + 1;
            int ownedDuplicates = GetOwnedDuplicatesCount();
            SetText(_textEnhanceRequirement, $"{ownedDuplicates} / {requiredDuplicates}");
        }

        if (_currentLevel >= maxLevel)
        {
            SetText(_textLevelUpRequirement, "최대레벨");
        }

        else
        {
            long ownedExp = SaveManager.Instance.CurrentSaveData.Exp;
            long requiredExp = _levelUpExp;
            SetText(_textLevelUpRequirement, $"{ownedExp} / {requiredExp}");
        }
    }

    private void OnClickEnhance()
    {
        if (_characterSaveData.Rank >= 5)
        {
            Debug.LogWarning("이미 최대 강화 단계입니다.");
            return;
        }

        int required = _characterSaveData.Rank + 1;
        int owned = GetOwnedDuplicatesCount();

        if (owned < required)
        {
            Debug.LogError($"강화 재료가 부족합니다. (필요: {required}, 보유: {owned})");
            return;
        }

        int consumed = 0;

        for (int i = SaveManager.Instance.CurrentSaveData.OwnedCharacters.Count - 1; i >= 0; i--)
        {
            var character = SaveManager.Instance.CurrentSaveData.OwnedCharacters[i];

            if (character.BaseId == _characterSaveData.BaseId && character.UniqueId != _characterSaveData.UniqueId)
            {
                SaveManager.Instance.CurrentSaveData.OwnedCharacters.RemoveAt(i);
                SaveManager.Instance.CharacterDict.Remove(character.UniqueId);
                consumed++;

                if (consumed >= required) break;
            }
        }

        _characterSaveData.Rank++;
        SaveManager.Instance.SaveCurrentData();

        Debug.Log($"{_characterData.Name}이(가) {_characterSaveData.Rank}강으로 강화되었습니다!");
        UpdateHunterInfo();
    }

    private void OnClickLevelUp()
    {
        int maxLevel = GetMaxLevel(_characterData.Rarity, _characterSaveData.Rank);

        if (_currentLevel >= maxLevel)
        {
            Debug.LogError("최대 레벨이므로 더 이상 경험치를 사용할 수 없습니다.");
            return;
        }

        long ownedExp = SaveManager.Instance.CurrentSaveData.Exp;

        if (ownedExp < _levelUpExp)
        {
            Debug.LogWarning($"경험치가 부족합니다. (보유: {ownedExp} / 필요: {_levelUpExp})");
            return;
        }

        SaveManager.Instance.CurrentSaveData.Exp -= _levelUpExp;
        _characterSaveData.Exp += _levelUpExp;
        SaveManager.Instance.SaveCurrentData();

        Debug.Log($"{_characterData.Name} 레벨 업! (현재 레벨: {_currentLevel + 1})");
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