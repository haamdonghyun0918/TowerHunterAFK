using Cysharp.Threading.Tasks;
using UnityEngine;

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
        if (SaveManager.Instance != null)
        {
            await SaveManager.Instance.Init();
        }

        CharacterInventory charInven = new CharacterInventory();
        charInven.Init();

        EquipmentInventory equipInven = new EquipmentInventory();
        equipInven.Init();

        if (ExpeditionManager.Instance != null)
        {
            ExpeditionManager.Instance.OnRewardClaimed += HandleExpeditionRewardClaimed;
            await ExpeditionManager.Instance.Init();
        }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnStageChanged += HandleStageChanged;
            MapManager.Instance.OnStageCleared += HandleStageCleared;
            MapManager.Instance.OnStageFailed += HandleStageFailed;
            await MapManager.Instance.Init();
        }

        Debug.Log("게임 세팅 완료 게임 화면 출력");
    }

    private void OnDestroy()
    {
        if (ExpeditionManager.Instance != null)
        {
            ExpeditionManager.Instance.OnRewardClaimed -= HandleExpeditionRewardClaimed;
        }

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
        SaveManager.Instance.UpdateMaxClearedStage(MapManager.Instance.CurrentStage);
        NetworkManager.Instance.PlayerResourceService.RequestAddGold(1000);
        long totalGold = NetworkManager.Instance.PlayerResourceService.GetPlayerResourceViewModel().Gold;
        SaveManager.Instance.SaveGold(totalGold);
        Debug.Log("클리어 보상 1000골드 지급!");

        int nextStage = MapManager.Instance.CurrentStage + 1;
        MapManager.Instance.StartNewStage(nextStage).Forget();
    }

    private void HandleStageFailed()
    {
        Debug.Log("스쿼드가 전멸하여 스테이지 실패...");
        int currentStage = MapManager.Instance.CurrentStage;
        int rollBackStage = ((currentStage - 1) / 10) * 10;

        if (rollBackStage < 10)
        {
            rollBackStage = 1;
        }

        Debug.Log($"실패로 인해 {rollBackStage} 스테이지로 돌아갑니다...");
        MapManager.Instance.StartNewStage(rollBackStage).Forget();
    }

    private void HandleExpeditionRewardClaimed(long addedGold, string[] equipments)
    {
        if (equipments != null && equipments.Length > 0)
        {
            EquipmentUtils equipUtils = new EquipmentUtils();

            foreach (string equipBaseId in equipments)
            {
                equipUtils.AddEquipments(equipBaseId);
            }

            if (addedGold > 0)
            {
                long totalGold = NetworkManager.Instance.PlayerResourceService.GetPlayerResourceViewModel().Gold;
                SaveManager.Instance.SaveGold(totalGold);
            }

        }
    }
}