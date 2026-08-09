using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

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

    private void Start()
    {
        StartGame().Forget();
    }

    private async UniTaskVoid StartGame()
    {
        Debug.Log("게임 시작중....=>로딩 화면");
        await SaveManager.Instance.Init();

        MapManager.Instance.OnStageChanged += HandleStageChanged;
        MapManager.Instance.OnStageCleared += HandleStageCleared;
        MapManager.Instance.OnStageFailed += HandleStageFailed;

        await MapManager.Instance.Init();
        Debug.Log("게임 세팅 완료 게임 화면 출력");
    }

    private void OnDestroy()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged -= HandleStageChanged;
            MapManager.Instance.OnStageCleared -= HandleStageCleared;
            MapManager.Instance.OnStageFailed -= HandleStageFailed;
        }

    }

    private void HandleStageChanged(int stage)
    {
        Debug.Log($"{stage} 스테이지입니다.");
    }

    private void HandleStageCleared()
    {
        Debug.Log("스테이지 클리어");
        NetworkManager.Instance.PlayerResourceService.RequestAddGold(1000);
        Debug.Log("클리어 보상 1000골드 지급!");
    }

    private void HandleStageFailed()
    {
        Debug.Log("스쿼드가 전멸하여 스테이지 실패...");
    }
}