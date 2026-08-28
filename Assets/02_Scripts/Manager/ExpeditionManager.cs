using UnityEngine;
using Cysharp.Threading.Tasks;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }

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