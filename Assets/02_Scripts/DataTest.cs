using UnityEngine;

public class DataTest : MonoBehaviour
{
    private void OnEnable()
    {
        StartDataTest();
    }

    public static void StartDataTest()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError($"[DataTest] GameDataManager가 NULL입니다.");
            return;
        }
        //바뀐 데이터 메니저의 양식에 따라 수정
        //var character = GameDataManager.Instance.GetCharacterData("character_Test_01");
        var character = GameDataManager.Instance.GetData<CharacterData>("character_Test_01");

        if (character != null)
        {
            Debug.Log($"로드된 캐릭터 이름: {character.Name}");
            Debug.Log($"로드된 캐릭터 스탯ID: {character.BaseStatDataId}");
            Debug.Log($"로드된 캐릭터 프리팹 경로: {character.PrefabPath}");
            Debug.Log($"로드된 캐릭터 경험치량: {character.Exp}");
        }

        var baseStatDataId = character.BaseStatDataId;
        if (string.IsNullOrEmpty(baseStatDataId) == true)
        {
            Debug.LogError($"베이스 스탯이 없는 캐릭터입니다.");
            return;
        }

        //var baseStatData = GameDataManager.Instance.GetBaseStatData(baseStatDataId);
        var baseStatData = GameDataManager.Instance.GetData<BaseStatData>(baseStatDataId);
        if (baseStatData == null)
        {
            Debug.Log("스탯 데이터를 찾을 수 없습니다.");
            return;
        }

        Debug.Log($"로드된 캐릭터 기본 공격력: {baseStatData.BaseAtk}");
        Debug.Log($"로드된 캐릭터 기본 체력: {baseStatData.BaseHp}");
        Debug.Log($"로드된 캐릭터 기본 공격속도: {baseStatData.BaseAtkSpeed}");
        Debug.Log($"로드된 캐릭터 기본 마나량: {baseStatData.BaseMp}");
    }
}
