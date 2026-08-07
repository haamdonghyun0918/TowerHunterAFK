using UnityEngine;

public class JIDataTest : MonoBehaviour
{
    private void Start()
    {
        // 전체 조회
        var all = GameDataManager.Instance.GetAllData<TestData>();
        Debug.Log($"총 {all.Count}개");

        foreach (var d in all)
            Debug.Log($"Id={d.Id}, Name={d.Name}, Desc={d.Description}");

        // 단일 조회
        var one = GameDataManager.Instance.GetData<TestData>("aaa");
        Debug.Log(one != null ? $"찾음: {one.Name}" : "못 찾음");

        // 없는 id. null이 나와야 정상
        var none = GameDataManager.Instance.GetData<TestData>("zzz");
        Debug.Log(none == null ? "없는 id 처리 정상" : "이상함");
    }
}
