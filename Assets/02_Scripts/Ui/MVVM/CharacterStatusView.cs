using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusView : MonoBehaviour
{
    [SerializeField] private int _slotIndex;

    [SerializeField] private GameObject Root_CharacterStatus;
    [SerializeField] private Slider Slider_Hp;

    private CharacterStatusViewModel _characterStatusViewModel;

    private void Start()
    {
        Init();   
    }

    private void OnEnable()
    {
        if(_characterStatusViewModel != null)
        {
            Bind();
            UpdateView();
        }
    }

    private void OnDisable()
    {
        UnBind();
    }

    private void Init()
    {
        if (Root_CharacterStatus == null)
        {
            Debug.LogError("[CharacterStatusView]: Root_CharacterStatus가 연결되지 않았습니다.");
            return;
        }
        if (Slider_Hp == null)
        {
            Debug.LogError("[CharacterStatusView]: Slider_Hp가 연결되지 않았습니다.");
            return;
        }

        if (NetworkManager.Instance == null || NetworkManager.Instance.CharacterStatusService == null)
        {
            Debug.LogError("[CharacterStatusView] CharacterStatusService가 없습니다.");
            return;
        }

        _characterStatusViewModel = NetworkManager.Instance.CharacterStatusService.GetCharacterStatusViewModel(_slotIndex);

        if(_characterStatusViewModel == null)
        {
            Debug.LogError($"[CharacterStatusView] CharacterStatusViewModel을 가져오지 못했습니다. SlotIndex: {_slotIndex}");
            return;
        }

        Slider_Hp.interactable = false;

        Bind();
        UpdateView();
    }

    private void Bind()
    {
        if(_characterStatusViewModel == null)
        {
            return;
        }

        _characterStatusViewModel.PropertyChanged -= OnPropertyChanged;
        _characterStatusViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void UnBind()
    {
        if(_characterStatusViewModel == null)
        {
            return;
        }
        _characterStatusViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if(eventArgs.PropertyName == nameof(CharacterStatusViewModel.CurrentHp) 
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.MaxHp) 
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.HpRatio) 
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.IsActive))
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        if(_characterStatusViewModel == null || Root_CharacterStatus == null || Slider_Hp == null)
        {
            return;
        }

        Root_CharacterStatus.SetActive(_characterStatusViewModel.IsActive);

        if(_characterStatusViewModel.IsActive == false)
        {
            return;
        }
        Slider_Hp.normalizedValue = Mathf.Clamp01(_characterStatusViewModel.HpRatio);
    }
}
