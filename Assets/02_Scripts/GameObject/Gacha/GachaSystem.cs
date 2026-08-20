using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    private List<CharacterData> _allCharacterData = new List<CharacterData>();
    private List<CharacterData> _HigherCharacterData = new List<CharacterData>();

    public static GachaSystem _Instance;

    private int _drawCharacterCount;
    private bool _isDrawTenCharacter;
    private int _totalHigherGachaWeight;
    private int _totalGachaWeight;

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        SetVariable();
    }

    private void SetVariable()
    {
        _drawCharacterCount = 0;
        _isDrawTenCharacter = false;
    }

    private void GetAllCharacterData()
    {
        _allCharacterData = GameDataManager.Instance.GetAllData<CharacterData>();
        SetTotalGachaWeight();
    }

    private void SetTotalGachaWeight()
    {
        foreach (var characterData in _allCharacterData)
        {
            _totalGachaWeight += characterData.GachaWeight;
        }
    }

    private void GetHigherCharacterData()
    {
        if (_allCharacterData == null || _allCharacterData.Count == 0)
        {
            _allCharacterData = GameDataManager.Instance.GetAllData<CharacterData>();
        }

        _HigherCharacterData.Clear();

        string LowRarity = "C";

        foreach (var characterData in _allCharacterData)
        {
            if (characterData.Rarity != LowRarity)
            {
                _HigherCharacterData.Add(characterData);
            }
        }

        SetHigherTotalGachaWeight();
    }

    private void SetHigherTotalGachaWeight()
    {
        foreach (var characterData in _HigherCharacterData)
        {
            _totalHigherGachaWeight += characterData.GachaWeight;
        }
    }

    private CharacterData DrawSingleCharacter()
    {
        if (_allCharacterData == null || _allCharacterData.Count == 0)
        {
            GetAllCharacterData();
        }

        if (_HigherCharacterData == null || _HigherCharacterData.Count == 0)
        {
            GetHigherCharacterData();
        }

        if (_allCharacterData == null || _allCharacterData.Count == 0)
        {
            Debug.LogError($"[GachaSystem] 캐릭터 데이터를 불러오지 못했습니다.");
            return null;
        }

        int totalGachaWeight = 0;

        if (_isDrawTenCharacter == true)
        {
            totalGachaWeight = _totalHigherGachaWeight;
        }

        else
        {
            totalGachaWeight = _totalGachaWeight;
        }

        int randomValue = DrawRandomValue(totalGachaWeight);

        return DrawCharacter(randomValue);
    }

    private List<CharacterData> DrawMultipleCharacter(int count = 10)
    {
        List<CharacterData> characters = new List<CharacterData>();

        for (int i = 0; i < count; i++)
        {
            var drawnCharacter = DrawSingleCharacter();
            if (drawnCharacter != null)
            {
                characters.Add(drawnCharacter);
            }
        }

        return characters;
    }

    private int DrawRandomValue(int totalWeight)
    {
        return Random.Range(0, totalWeight);
    }

    private CharacterData DrawCharacter(int randomValue)
    {
        int currentWeightSum = 0;

        if (_isDrawTenCharacter == true)
        {
            foreach (var character in _HigherCharacterData)
            {
                currentWeightSum += character.GachaWeight;
                if (randomValue < currentWeightSum)
                {
                    return DrawnCharacter(currentWeightSum, randomValue, character);
                }
            }
        }

        else
        {
            foreach (var character in _allCharacterData)
            {
                currentWeightSum += character.GachaWeight;
                if (randomValue < currentWeightSum)
                {
                    return DrawnCharacter(currentWeightSum, randomValue, character);
                }
            }
        }

        return null;
    }

    private CharacterData DrawnCharacter(int currentWeightSum, int randomValue, CharacterData character)
    {
        LogDrawnCharacter(character);

        PlayDrawAnimation(character);

        OpenCharacterCardUI(character);

        // 실제로 저장되어야 하므로 CharacterUtils에서 Add를 통하여 인스턴스 Id를 가지며 인벤토리에 들어가도록 추가
        HunterUtil hunterUtil = new HunterUtil();
        hunterUtil.AddCharacters(character.Id);

        _drawCharacterCount++;
        Debug.Log($"{_drawCharacterCount}개의 캐릭터를 뽑았습니다.");

        if (_isDrawTenCharacter == true)
        {
            _drawCharacterCount = 0;
        }

        CheckDrawTenCharacters();
        return character;
    }

    private void OpenCharacterCardUI(CharacterData character)
    {
        //[TODO] 카드 UI 오픈
        string id = character.Id;
        int starLevel = character.StarLevel;
        string name = character.Name;
        string rarity = character.Rarity;
        var baseStatData = GameDataManager.Instance.GetData<BaseStatData>(character.BaseStatDataId);
        int baseAtk = baseStatData.BaseAtk;
        int baseDef = baseStatData.BaseDef;
        int baseAtkSpeed = baseStatData.BaseAtkSpeed;
        int baseHp = baseStatData.BaseHp;
    }

    private void PlayDrawAnimation(CharacterData character)
    {
        //[TODO] 등급에 따라 뽑히는 애니메이션 다르게 재생
        string Rarity = character.Rarity;
        switch (Rarity)
        {
            case "A":
                {
                    Debug.Log("축하합니다! A등급이 뽑혔습니다.");
                }
                break;
            case "B":
                {
                    Debug.Log("축하합니다! B등급이 뽑혔습니다.");
                }
                break;
            case "C":
                {
                    Debug.Log("축하합니다! C등급이 뽑혔습니다.");
                }
                break;
        }
    }

    private void LogDrawnCharacter(CharacterData character)
    {
        string prefix = _isDrawTenCharacter ? "[최소 등급 보장] " : "";
        Debug.Log($"{prefix}뽑힌 캐릭터: {character.Id}, 등급: {character.Rarity}");
    }

    private void CheckDrawTenCharacters()
    {
        if (_drawCharacterCount == 9)
        {
            _isDrawTenCharacter = true;
            return;
        }

        _isDrawTenCharacter = false;
    }


    // 테스트용 치트 함수 ==============================================

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DrawSingleCharacter();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            DrawMultipleCharacter();
        }
    }
}
