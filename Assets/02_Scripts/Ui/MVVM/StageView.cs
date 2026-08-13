using TMPro;
using UnityEngine;

public class StageView : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_CurrentStage;

    private StageViewModel _stageViewModel;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if(Text_CurrentStage == null)
        {
            Debug.LogError("[StageView]: Text_CurrentStage가 연결되지 않았습니다.");
            return;
        }

        if(NetworkManager.Instance == null || NetworkManager.Instance.StageService == null)
        {
            Debug.LogError("[StageView]: StageService가 없습니다.");
            return;
        }

        _stageViewModel = NetworkManager.Instance.StageService.GetStageViewModel();
        
    }
}
