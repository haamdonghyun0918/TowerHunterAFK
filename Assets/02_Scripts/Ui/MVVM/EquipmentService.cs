using UnityEngine;

public class EquipmentService
{
    private EquipmentDetailViewModel _equipmentDetailViewModel;

    public EquipmentDetailViewModel GetEquipmentDetailViewModel()
    {
        if (_equipmentDetailViewModel == null)
        {
            _equipmentDetailViewModel = new EquipmentDetailViewModel();
        }
        return _equipmentDetailViewModel;
    }

    private EquipmentEnhanceViewModel _equipmentEnhanceViewModel;

    public EquipmentEnhanceViewModel GetEquipmentEnhanceViewModel()
    {
        if (_equipmentEnhanceViewModel == null)
        {
            _equipmentEnhanceViewModel = new EquipmentEnhanceViewModel();
        }
        return _equipmentEnhanceViewModel;
    }

    private EquipmentDisassembleViewModel _equipmentDisassembleViewModel;

    public EquipmentDisassembleViewModel GetEquipmentDisassembleViewModel()
    {
        if (_equipmentDisassembleViewModel == null)
        {
            _equipmentDisassembleViewModel = new EquipmentDisassembleViewModel();
        }
        return _equipmentDisassembleViewModel;
    }

    public void SetDetailTarget(string uniqueId)
    {
        if (SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false)
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return;
        }

        EquipMentData baseData = GameDataManager.Instance.GetData<EquipMentData>(saveData.BaseId);
        if (baseData == null) return;

        var vm = GetEquipmentDetailViewModel();

        vm.TargetEquipmentUniqueId = uniqueId;
        vm.ItemIconAddress = baseData.IconAddress;
        vm.ItemName = baseData.Name;

        int totalAtk = baseData.BuffAtk + (saveData.EnhanceLevel * 5);
        int totalDef = baseData.BuffDef + (saveData.EnhanceLevel * 3);
        int totalSpd = baseData.BuffAtkSpeed;

        vm.TotalStatText = $"공격력 : {totalAtk}\n방어력 : {totalDef}\n속도 : {totalSpd}";
    }

    public void SetEnhanceTarget(string uniqueId)
    {
        if (SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false)
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return;
        }

        EquipMentData baseData = GameDataManager.Instance.GetData<EquipMentData>(saveData.BaseId);

        if (baseData == null)
        {
            Debug.LogError($"[EquipmentService] 해당 baseData의 장비를 찾을 수 없습니다: {baseData}");
            return;
        }

        var vm = GetEquipmentEnhanceViewModel();

        vm.TargetEquipmentUniqueId = uniqueId;
        vm.ItemIconAddress = baseData.IconAddress;
        vm.ItemName = baseData.Name;
        vm.EnhanceLevel = saveData.EnhanceLevel;

        int totalAtk = baseData.BuffAtk + (saveData.EnhanceLevel * 5);
        vm.TotalAtkText = $"공격력: {totalAtk}";

        int totalDef = baseData.BuffDef + (saveData.EnhanceLevel * 3);
        vm.TotalDefText = $"방어력: {totalDef}";

        //테스트를 위해 일단은 골드만 요구하도록 로직 구현
        long cost = (saveData.EnhanceLevel + 1) * 10;
        vm.CostText = $"{cost} Gold";


        //==================테스트용 하드코딩 데이터.
        //사용시 본 메서드의 위쪽 코드 전부 주석처리 하고 사용할 것.
        //아이콘 주소가 없어서 에러가 날 수도 있음.
        //var vm = GetEquipmentEnhanceViewModel();

        //vm.TargetEquipmentUniqueId = uniqueId;
        //vm.ItemName = "테스트 장비";
        //vm.EnhanceLevel = 5;
        //vm.TotalAtkText = "공격력: 100";
        //vm.CostText = "100 Gold";
    }

    public bool RequestEnhance(string uniqueId)
    {
        if (SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false)
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return false;
        }

        EquipMentData baseData = GameDataManager.Instance.GetData<EquipMentData>(saveData.BaseId);
        if (baseData == null)
        {
            Debug.LogError($"[EquipmentService] 해당 장비의 BaseData를 찾을 수 없습니다: {saveData.BaseId}");
            return false;
        }

        long cost = (saveData.EnhanceLevel + 1) * 10;
        bool isGoldUsed = NetworkManager.Instance.PlayerResourceService.RequestUseGold(cost);

        if (isGoldUsed == false)
        {
            Debug.LogWarning("골드가 부족합니다.");
            return false;
        }

        saveData.EnhanceLevel += 1;

        SetEnhanceTarget(uniqueId);

        SaveManager.Instance.SaveCurrentData();

        Debug.Log("강화 성공");

        return true;
    }

    public void SetDisassembleTarget(string uniqueId)
    {
        if (SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false)
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return;
        }

        EquipMentData baseData = GameDataManager.Instance.GetData<EquipMentData>(saveData.BaseId);

        if (baseData == null)
        {
            Debug.LogError($"[EquipmentService] 해당 baseData의 장비를 찾을 수 없습니다: {baseData}");
            return;
        }

        var vm = GetEquipmentDisassembleViewModel();

        vm.TargetEquipmentUniqueId = uniqueId;
        vm.ItemIconAddress = baseData.IconAddress;
        vm.ItemName = $"{baseData.Name}+{saveData.EnhanceLevel}";

        //테스트용 임시 획득 재화(골드) 나중에 마석 관련 데이터, 로직 나오면 바꿀 것.
        long reward = (baseData.Price) / 10;
        vm.RewardText = $"장비 분해 시 획득 재화\n{reward} Gold\n(테스트용 골드 지급/추후 마석으로 바꿀 것)";
        
    }

    public bool RequestDisassemble(string uniqueId)
    {
        if (SaveManager.Instance.EquipmentDict.TryGetValue(uniqueId, out EquipmentSaveData saveData) == false)
        {
            Debug.LogError($"[EquipmentService] 해당 UID의 장비를 찾을 수 없습니다: {uniqueId}");
            return false;
        }

        EquipMentData baseData = GameDataManager.Instance.GetData<EquipMentData>(saveData.BaseId);
        if (baseData == null)
        {
            Debug.LogError($"[EquipmentService] 해당 장비의 BaseData를 찾을 수 없습니다: {saveData.BaseId}");
            return false;
        }

        long reward = (baseData.Price) / 10;

        //이게 장비 아이템 제거하는 함수인지 불명확함. 물어볼것.
        NetworkManager.Instance.PlayerResourceService.RequestUseEquipments(saveData.UniqueId);

        //이건 직접 세이브 매니저를 통해서 제거하는 코드인데
        //인벤토리 통해서 제거하는게 나을 것 같으니 테스트 용에서만 사용할것.
        //SaveManager.Instance.EquipmentDict.Remove(uniqueId);
        //var equipList = SaveManager.Instance.CurrentSaveData.OwnedEquipments;
        //var targetEquip = equipList.Find(e => e.UniqueId == uniqueId);
        //if (targetEquip != null)
        //{
        //    equipList.Remove(targetEquip);
        //}

        NetworkManager.Instance.PlayerResourceService.RequestAddGold(reward);

        SaveManager.Instance.SaveCurrentData();

        Debug.Log("분해 성공");

        return true;
    }

}
