using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CharacterStatusView : MonoBehaviour
{
    [SerializeField] private int _slotIndex;

    [SerializeField] private GameObject Root_CharacterStatus;
    [SerializeField] private Slider Slider_Hp;

    [SerializeField] private TMP_Text Text_HunterName;

    private CharacterStatusViewModel _characterStatusViewModel;

    [SerializeField] private Image Image_HunterIcon;

    [SerializeField] private List<GameObject> SkillCostList = new List<GameObject>();

    private string _currentCircleIconPath = "";

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        if (_characterStatusViewModel != null)
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
        if (Text_HunterName == null)
        {
            Debug.LogError($"[CharacterStatusView] Text_HunterName이 연결되지 않았습니다. SlotIndex: {_slotIndex}");
            return;
        }

        if(Image_HunterIcon == null)
        {
            Debug.LogError($"[CharacterStatusView] Image_HunterIcon이 연결되지 않았습니다. SlotIndex: {_slotIndex}");
            return;
        }


        if (NetworkManager.Instance == null || NetworkManager.Instance.CharacterStatusService == null)
        {
            Debug.LogError("[CharacterStatusView] CharacterStatusService가 없습니다.");
            return;
        }

        _characterStatusViewModel = NetworkManager.Instance.CharacterStatusService.GetCharacterStatusViewModel(_slotIndex);

        if (_characterStatusViewModel == null)
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
        if (_characterStatusViewModel == null)
        {
            return;
        }

        _characterStatusViewModel.PropertyChanged -= OnPropertyChanged;
        _characterStatusViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void UnBind()
    {
        if (_characterStatusViewModel == null)
        {
            return;
        }
        _characterStatusViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(CharacterStatusViewModel.CurrentHp)
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.MaxHp)
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.HpRatio)
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.IsActive)
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.CurrentSkillCost)
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.MaxSkillCost)
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.Name)
            || eventArgs.PropertyName == nameof(CharacterStatusViewModel.CircleIconPath))
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        if (_characterStatusViewModel == null || Root_CharacterStatus == null || Slider_Hp == null || Text_HunterName == null)
        {
            return;
        }

        Root_CharacterStatus.SetActive(_characterStatusViewModel.IsActive);

        UpdateHunterIconView();

        if (_characterStatusViewModel.IsActive == false)
        {
            return;
        }
        Slider_Hp.normalizedValue = Mathf.Clamp01(_characterStatusViewModel.HpRatio);
        Text_HunterName.text = _characterStatusViewModel.Name;

        UpdateSkillCostView();
    }

    private void UpdateHunterIconView()
    {
        if(Image_HunterIcon == null || _characterStatusViewModel == null)
        {
            return;
        }
        string circleIconPath = _characterStatusViewModel.CircleIconPath;

        if(_currentCircleIconPath == circleIconPath)
        {
            return;
        }

        _currentCircleIconPath = circleIconPath;
        Image_HunterIcon.sprite = null;
        Image_HunterIcon.gameObject.SetActive(false);

        if(string.IsNullOrEmpty(circleIconPath) )
        {
            return;
        }
        if(ResourceManager.Instance == null)
        {
            _currentCircleIconPath = null;

            Debug.LogWarning($"[CharacterStatusView] ResourceManager가 없어 아이콘을 불러올 수 없습니다. SlotIndex: {_slotIndex}");

            return;
        }

        CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();

        LoadHunterIconAsync(circleIconPath, destroyToken).Forget();

        
    }

    private async UniTask LoadHunterIconAsync(string circleIconPath, CancellationToken destroyToken)
    {
        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(circleIconPath);

        if(destroyToken.IsCancellationRequested)
        {
            return;
        }

        if(Image_HunterIcon == null || _characterStatusViewModel == null)
        {
            return;
        }

        if (_currentCircleIconPath != circleIconPath || _characterStatusViewModel.CircleIconPath != circleIconPath)
        { 
            return ;
        }

        if(loadedSprite == null)
        {
            Debug.LogWarning($"[CharacterStatusView] 원형 아이콘 로드 실패: {circleIconPath}");
            return;
        }

        Image_HunterIcon.sprite = loadedSprite;
        Image_HunterIcon.gameObject.SetActive(true);
    }

    private void UpdateSkillCostView()
    {
        int activeCostCount = Mathf.Clamp(_characterStatusViewModel.CurrentSkillCost, 0, Mathf.Min(_characterStatusViewModel.MaxSkillCost, SkillCostList.Count));

        for(int i = 0; i< SkillCostList.Count; i++)
        {
            if(SkillCostList[i] == null)
            {
                continue;
            }

            bool shouldBeActive = i < activeCostCount;

            SkillCostList[i].SetActive(shouldBeActive);
        }
    }
}
