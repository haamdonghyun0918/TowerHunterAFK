using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    private float _inactivityTimer = 0f;
    private const float _sleepModeTimer = 60f;
    private bool _isSleepMode = false;

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

    private void Update()
    {
        CheckSleepMode();
    }

    private async UniTaskVoid StartGame()
    {
        Debug.Log("게임 시작중....=>로딩 화면");
        if (SaveManager.Instance != null)
        {
            await SaveManager.Instance.Init();
        }
        else
        {
            Debug.LogError("[GameFlowManager] SaveManager가 없습니다.");
            return;
        }

        if (NetworkManager.Instance == null || NetworkManager.Instance.StageService == null || NetworkManager.Instance.PlayerResourceService == null)
        {
            Debug.LogError("[GameFlowManager] NetworkManager의 Service가 초기화되지 않았습니다.");
            return;
        }

        NetworkManager.Instance.PlayerResourceService.SetGoldOnLoad(SaveManager.Instance.CurrentSaveData.Gold);
        NetworkManager.Instance.PlayerResourceService.SetDiamondOnLoad(SaveManager.Instance.CurrentSaveData.Diamond);

        CharacterInventory charInven = new CharacterInventory();
        charInven.Init();

        EquipmentInventory equipInven = new EquipmentInventory();
        equipInven.Init();

        if (ExpeditionManager.Instance != null)
        {
            //ExpeditionManager.Instance.OnRewardClaimed += HandleExpeditionRewardClaimed;
            await ExpeditionManager.Instance.Init();
        }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged += HandleStageChanged;
            MapManager.Instance.OnStageCleared += HandleStageCleared;
            MapManager.Instance.OnStageFailed += HandleStageFailed;
            await MapManager.Instance.Init();
        }
        else
        {
            Debug.LogError("[GameFlowManager] MapManager가 없습니다.");
            return;
        }
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
        if (ObjectManager.Instance != null)
        {
            ObjectManager.Instance.SpawnEntities(stage).Forget();
        }
    }

    private void HandleStageCleared()
    {
        Debug.Log("스테이지 클리어");

        if (NetworkManager.Instance == null || NetworkManager.Instance.StageService == null)
        {
            Debug.LogError("[GameFlowManager] StageService가 없습니다.");
            return;
        }

        StageService stageService = NetworkManager.Instance.StageService;

        stageService.UpdateMaxClearedStage(MapManager.Instance.CurrentStage);

        NetworkManager.Instance.PlayerResourceService.RequestAddGold(1000);
        Debug.Log("클리어 보상 1000골드 지급!");

        int currentClearedStage = stageService.GetStageViewModel().CurrentStage;
        if (currentClearedStage % 10 == 0)
        {
            NetworkManager.Instance.PlayerResourceService.RequestAddDiamond(500);
            Debug.Log($"보스 층인 {currentClearedStage} Floor 를 클리어 하셨습니다! 500 다이아몬드가 추가됩니다!");
        }

        int preSaveStage = stageService.GetStageViewModel().CurrentStage;
        stageService.RequestGoNextStage();

        int nextStage = preSaveStage + 1;
        MapManager.Instance.StartNewStage(nextStage).Forget();
    }

    private void HandleStageFailed()
    {
        Debug.Log("스쿼드가 전멸하여 스테이지 실패...");

        if (NetworkManager.Instance == null || NetworkManager.Instance.StageService == null)
        {
            Debug.LogError("[GameFlowManager]: StageService가 없습니다.");
            return;
        }

        int currentStage = MapManager.Instance.CurrentStage;
        StageService stageService = NetworkManager.Instance.StageService;

        stageService.GoToSafeStage();

        int rollBackStage = stageService.GetStageViewModel().CurrentStage;

        Debug.Log($"실패로 인해 {rollBackStage} 스테이지로 돌아갑니다...");
        MapManager.Instance.StartNewStage(rollBackStage).Forget();
    }

    public bool GetSleepMode()
    {
        return _isSleepMode;
    }

    private void CheckSleepMode()
    {
        bool isInputDetected = (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1));

        if (isInputDetected)
        {
            if (_isSleepMode)
            {
                ExitSleepMode();
            }

            _inactivityTimer = 0f;
        }

        else
        {
            if (_isSleepMode == false)
            {
                _inactivityTimer += Time.deltaTime;

                if (_inactivityTimer >= _sleepModeTimer)
                {
                    EnterSleepMode();
                }
            }
        }
    }

    private void EnterSleepMode()
    {
        _isSleepMode = true;

        Application.targetFrameRate = 15;

        if (UiManager.Instance != null)
        {
            UiManager.Instance.OpenUi<SleepModeUi>().Forget();
        }

        Debug.Log("[GameFlowManager] 절전 모드 진입/ 15 프레임으로 진행");
    }

    private void ExitSleepMode()
    {
        _isSleepMode = false;

        Application.targetFrameRate = 60;

        if (UiManager.Instance != null)
        {
            UiManager.Instance.CloseUi<SleepModeUi>();
        }

        _inactivityTimer = 0f;
        Debug.Log("[GameFlowManager] 절전 모드 해제: 60프레임으로 복구");
    }
}