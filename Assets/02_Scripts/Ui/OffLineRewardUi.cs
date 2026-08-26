using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OffLineRewardUi : UiBase
{
    [Header("Texts")]
    [SerializeField] private TMP_Text _textTime;
    [SerializeField] private TMP_Text _textGold;
    [SerializeField] private TMP_Text _textExp;
    [SerializeField] private TMP_Text _textMagicStone;

    [Header("Buttons")]
    [SerializeField] private UiButton _buttonClaim;
    [SerializeField] private UiButton _buttonClose;

    [Header("Equipment Scroll")]
    [SerializeField] private Transform _content;
    private const string EquipmentSlotAddress = "EquipmentSlot";

    private OffLineRewardService _rewardService;
    private List<GameObject> _createdSlots = new List<GameObject>();

    private void OnEnable()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[OffLineRewardUi] NetworkManager.Instance가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.OffLineRewardService == null)
        {
            Debug.LogError("[OffLineRewardUi] OfflineRewardService가 없습니다.");
            return;
        }

        _rewardService = NetworkManager.Instance.OffLineRewardService;

        _rewardService.CalculateOfflineReward();

        if (_buttonClaim != null)
        {
            _buttonClaim.BindOnClickButtonEvent(OnClickClaim);
        }

        if (_buttonClose != null)
        {
            _buttonClose.BindOnClickButtonEvent(OnClickClose);
        }

        UpdateUiAsync().Forget();
    }

    private void OnDisable()
    {
        if (_buttonClaim != null)
        {
            _buttonClaim.UnBindOnClickButtonEvent(OnClickClaim);
        }

        if (_buttonClose != null)
        {
            _buttonClose.UnBindOnClickButtonEvent(OnClickClose);
        }
    }

    private async UniTaskVoid UpdateUiAsync()
    {
        if (_textTime != null)
        {
            _textTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}",(int)_rewardService.OfflineTime.TotalHours, _rewardService.OfflineTime.Minutes, _rewardService.OfflineTime.Seconds);
        }

        if (_textGold != null)
        {
            _textGold.text = $"+{_rewardService.RewardGold.ToString("N0")}";
        }

        if (_textExp != null)
        {
            _textExp.text = $"+{_rewardService.RewardExp.ToString("N0")}";
        }

        if (_textMagicStone != null)
        {
            _textMagicStone.text = $"+{_rewardService.RewardMagicStone.ToString("N0")}";
        }

        for (int i = 0; i < _createdSlots.Count; i++)
        {
            if (_createdSlots[i] != null)
            {
                Destroy(_createdSlots[i]);
            }
        }

        _createdSlots.Clear();

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("[OffLineRewardUi] ResourceManager.Instance가 없습니다.");
            return;
        }

        for (int i = 0; i < _rewardService.RewardEquipments.Count; i++)
        {
            EquipmentData equipData = _rewardService.RewardEquipments[i];

            if (equipData == null)
            {
                continue;
            }

            GameObject slotObj = await ResourceManager.Instance.Instantiate(EquipmentSlotAddress, _content, true);

            if (slotObj != null)
            {
                slotObj.transform.localScale = Vector3.one;
                EquipmentSlotView slotView = slotObj.GetComponent<EquipmentSlotView>();

                if (slotView != null)
                {
                    EquipmentSaveData fakeSaveData = new EquipmentSaveData();
                    fakeSaveData.UniqueId = "temp";
                    fakeSaveData.BaseId = equipData.Id;
                    fakeSaveData.EnhanceLevel = 0;

                    EquipmentModel fakeModel = new EquipmentModel(fakeSaveData, equipData);
                    EquipmentSlotViewModel fakeViewModel = new EquipmentSlotViewModel(fakeModel);

                    slotView.SetUp(fakeViewModel, null);
                }

                _createdSlots.Add(slotObj);
            }
        }
    }

    private void OnClickClaim()
    {
        if (_rewardService != null)
        {
            _rewardService.ClaimRewards();
        }

        if (UiManager.Instance != null)
        {
            UiManager.Instance.CloseUi<OffLineRewardUi>();
        }
    }

    private void OnClickClose()
    {
        if (UiManager.Instance != null)
        {
            UiManager.Instance.CloseUi<OffLineRewardUi>();
        }
    }
}