using UnityEngine;

public class CharacterUtils : MonoBehaviour
{
    public static CharacterUtils Instance { get; private set; }

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

    public void AddCharacters(string baseCharacterId)
    {
        CharacterData charData = GameDataManager.Instance.GetData<CharacterData>(baseCharacterId);

        if (charData == null)
        {
            Debug.LogError($"[CharacterUtils] 존재하지 않는 캐릭터 ID입니다: {baseCharacterId}");
            return;
        }

        SaveManager.Instance.CurrentSaveData.RecentCharacterUid += 1;
        uint currentUid = SaveManager.Instance.CurrentSaveData.RecentCharacterUid;

        CharacterSaveData newCharacter = new CharacterSaveData
        {
            UniqueId = "CH_" + currentUid.ToString(),
            BaseId = baseCharacterId,
            Level = 1,
            Exp = 0,
            EquippedWeaponUid = "",
            EquippedArmorUid = "",
            EquippedAccessoryUid = ""
        };

        SaveManager.Instance.CurrentSaveData.OwnedCharacters.Add(newCharacter);
        SaveManager.Instance.SaveCurrentData();

        Debug.Log($"[CharacterUtils] 헌터 획득! 헌터 이름: {charData.Name}], 게임 상 헌터 아이디: {newCharacter.UniqueId})");
    }
}