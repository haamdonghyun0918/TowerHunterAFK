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

    private void OnEnable()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged += HandleStageChanged;
            MapManager.Instance.OnStageCleared += HandleStageCleared;
        }
        // TODO: 스쿼드 전멸시 이벤트 발생하면 구독
    }

    private void OnDisable()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged -= HandleStageChanged;
            MapManager.Instance.OnStageCleared -= HandleStageCleared;
        }
        //TODO: 스쿼드 전멸 이벤트 구독 해지
    }

    private async UniTaskVoid StartGame()
    {
        Debug.Log("게임 시작....");
        SaveManager.Instance.Init();
        MapManager.Instance.Init();
        await UniTask.Delay(500);
    }

    private void HandleStageChanged(int stage)
    {
        Debug.Log($"{stage} 스테이지입니다.");
    }

    private void HandleStageCleared()
    {
        Debug.Log("스테이지 클리어");
        SaveManager.Instance.AddGold(10000);
        Debug.Log("클리어 보상 10000골드 지급!");
    }

    private void OnFailedStage() // TODO: 메서드 이름 변경할 것 이벤트 구독을 했을 때
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.FailedStage();
        }
    }
}