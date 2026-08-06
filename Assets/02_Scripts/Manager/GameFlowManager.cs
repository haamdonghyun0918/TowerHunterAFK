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
            DontDestroyOnLoad(gameObject);
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
    }

    private void OnDisable()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged -= HandleStageChanged;
            MapManager.Instance.OnStageCleared -= HandleStageCleared;
        }
    }

    private async UniTaskVoid StartGame()
    {

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

    public void OnFailedStage()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.FailedStage();
        }
    }
}