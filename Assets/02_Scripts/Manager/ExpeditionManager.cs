using UnityEngine;
using Cysharp.Threading.Tasks;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }
    ////TODO: 헌터들의 데이터를 가져와야 함 + 헌터들을 통하여 스쿼드 짜는 로직 추가할 것
    //public event Action<ExpeditionData> OnExpeditionSelected;
    //public event Action OnExpeditionStarted;
    //public event Action OnExpeditionCompleted;
    //public event Action<long, string[]> OnRewardClaimed;
    //public event Action<int> OnExpeditionLevelNotEnough;

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

    public UniTask Init()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.ExpeditionService != null)
        {
            NetworkManager.Instance.ExpeditionService.Init();
        }

        else
        {
            Debug.LogError("[ExpeditionManager] ExpeditionService가 없습니다.");
        }

        Debug.Log("ExpeditionManager 호출 및 서비스 초기화 지시 완료");
        return UniTask.CompletedTask;
    }

}