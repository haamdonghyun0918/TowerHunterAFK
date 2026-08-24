using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EquipmentInventoryUi : UiBase
{
    [SerializeField] private Transform Content_Equipment;
    [SerializeField] private UiButton _buttonClose;

    private const string EquipmentSlotAddress = "EquipmentSlot";
    private readonly List<EquipmentSlotView> _createdSlots = new List<EquipmentSlotView>();

    private const int _firstSlotCount = 30;

    private EquipmentInventoryViewModel _viewModel;
    private bool _isRefreshing;
    private bool _refreshRequested;

    private void OnEnable()
    {
        BindCloseButton();

        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[EquipmentInventoryUi] NetworkManager가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[EquipmentInventoryUi] EquipmentService가 없습니다.");
            return;
        }

        _viewModel = NetworkManager.Instance.EquipmentService.GetEquipmentInventoryViewModel();
        BindViewModel();
        _viewModel.Refresh();
    }

    private void OnDisable()
    {
        UnbindViewModel();
        UnbindCloseButton();

        if (_viewModel != null)
        {
            _viewModel.RequestCancelEquipSelection();
        }
    }

    private void BindCloseButton()
    {
        if (_buttonClose == null)
        {
            return;
        }

        _buttonClose.UnBindOnClickButtonEvent(CloseEquipmentInventory);
        _buttonClose.BindOnClickButtonEvent(CloseEquipmentInventory);
    }

    private void UnbindCloseButton()
    {
        if (_buttonClose != null)
        {
            _buttonClose.UnBindOnClickButtonEvent(CloseEquipmentInventory);
        }
    }

    private void BindViewModel()
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.PropertyChanged -= OnPropertyChanged;
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void UnbindViewModel()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged;
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(EquipmentInventoryViewModel.Equipments))
        {
            RequestRefreshSlots();
        }
    }

    private void RequestRefreshSlots()
    {
        if (_isRefreshing)
        {
            _refreshRequested = true;
            return;
        }

        RefreshEquipmentSlotsAsync().Forget();
    }

    private async UniTaskVoid RefreshEquipmentSlotsAsync()
    {
        _isRefreshing = true;

        try
        {
            do
            {
                _refreshRequested = false;
                await RefreshEquipmentSlotsOnceAsync();
            }
            while (_refreshRequested);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async UniTask RefreshEquipmentSlotsOnceAsync()
    {
        if (_viewModel == null || Content_Equipment == null)
        {
            return;
        }

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("[EquipmentInventoryUi] ResourceManager가 없습니다.");
            return;
        }

        IReadOnlyList<EquipmentSlotViewModel> equipments = _viewModel.Equipments;

        int targetSlotCount = Mathf.Max(_firstSlotCount, equipments.Count);
        int currentCount = _createdSlots.Count;

        for (int i = currentCount; i < targetSlotCount; i++)
        {
            GameObject slotObject = await ResourceManager.Instance.Instantiate(EquipmentSlotAddress, Content_Equipment);

            if (slotObject == null)
            {
                Debug.LogError("[EquipmentInventoryUi] 장비 슬롯 생성에 실패했습니다.");
                return;
            }

            EquipmentSlotView slotView = slotObject.GetComponent<EquipmentSlotView>();

            if (slotView == null)
            {
                Debug.LogError("[EquipmentInventoryUi] EquipmentSlotView가 없습니다.");
                slotObject.SetActive(false);
                return;
            }

            _createdSlots.Add(slotView);
        }

        for (int i = 0; i < _createdSlots.Count; i++)
        {
            EquipmentSlotView slotView = _createdSlots[i];

            if (i < equipments.Count)
            {
                slotView.SetUp(equipments[i], OnClickEquipmentSlot);
                slotView.gameObject.SetActive(true);
            }
            else
            {
                slotView.SetUp(null, null);
                slotView.gameObject.SetActive(false);
            }
        }
    }

    private void OnClickEquipmentSlot(string uniqueId)
    {
        if (_viewModel == null)
        {
            return;
        }

        EquipmentSelectionResult result = _viewModel.RequestSelectEquipment(uniqueId);

        if (result == EquipmentSelectionResult.OpenDetail)
        {
            UiManager.Instance.OpenUi<EquipmentDetailUI>().Forget();
            return;
        }

        if (result == EquipmentSelectionResult.Equipped)
        {
            UiManager.Instance.CloseUi<EquipmentInventoryUi>();
        }
    }

    private void CloseEquipmentInventory()
    {
        if (_viewModel != null)
        {
            _viewModel.RequestCancelEquipSelection();
        }

        UiManager.Instance.CloseUi<EquipmentInventoryUi>();
    }
}