using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

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

    [Header("Buttons")]
    [SerializeField] private UiButton _buttonClose;

    private void OnEnable()
    {
        if (_buttonClose != null)
        {
            _buttonClose.UnBindOnClickButtonEvent(CloseHunterInfoUi);
            _buttonClose.BindOnClickButtonEvent(CloseHunterInfoUi);
        }
    }

    public async UniTaskVoid SetUp(string uniqueId, string baseId)
    {
        CharacterData huntData = GameDataManager.Instance.GetData<CharacterData>(baseId);
        BaseStatData baseStat = GameDataManager.Instance.GetData<BaseStatData>(huntData.BaseStatDataId);

        if (huntData == null || baseStat == null)
        {
            Debug.LogError("헌터의 데이터가 없거나 헌터의 기본 스텟이 없습니다.");
            return;
        }

        if (SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out var saveData))
        {
            //TODO: 캐릭터 레벨을 위하여 경험치 작업 해야함 현재는 하드코딩으로 1로 고정
            int currentLevel = 1;
            int currentRank = saveData.Rank;
            //TODO: 장비까지 나중에 추가되면 바로 연동되도록 계산식 후에 추가하기
            int finalAtk = baseStat.BaseAtk + (huntData.AtkPerLevel * (currentLevel - 1));
            int finalHp = baseStat.BaseHp + (huntData.HpPerLevel * (currentLevel - 1));
            int finalDef = baseStat.BaseDef + (huntData.DefPerLevel * (currentLevel - 1));
            int finalSpd = baseStat.BaseAtkSpeed;

            if (_textName != null)
            {
                _textName.text = huntData.Name;
            }

            if (_textHp != null)
            {
                _textHp.text = finalHp.ToString();
            }

            if (_textAtk != null)
            {
                _textAtk.text = finalAtk.ToString();
            }
            if (_textDef != null)
            {
                _textDef.text = finalDef.ToString();
            }

            if (_textSpd != null)
            {
                _textSpd.text = finalSpd.ToString();
            }

            if (_textCost != null)
            {
                _textCost.text = huntData.MaxSkillCost.ToString();
            }

            if (_textTier != null)
            {
                _textTier.text = huntData.Rarity;
            }

            if (_textRank != null)
            {
                _textRank.text = $"{currentRank} / 10";
            }

            if (_textLevel != null)
            {
                _textLevel.text = $"{currentLevel} / 10";
            }

            if (string.IsNullOrEmpty(huntData.IconPath) == false)
            {
                Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(huntData.IconPath);

                if (loadedSprite != null && _hunterProfileImage != null)
                {
                    _hunterProfileImage.sprite = loadedSprite;
                    _hunterProfileImage.gameObject.SetActive(true);
                }
            }

            else
            {
                if (_hunterProfileImage != null)
                {
                    _hunterProfileImage.sprite = null;
                    _hunterProfileImage.gameObject.SetActive(false);
                }
            }

        }
    }

    private void CloseHunterInfoUi()
    {
        UiManager.Instance.CloseUi<HunterInfoUi>();
    }
}