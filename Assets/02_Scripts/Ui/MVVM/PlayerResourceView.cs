using System.ComponentModel;
using TMPro;
using UnityEngine;
public class PlayerResourceView : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_Gold;
    [SerializeField] private TMP_Text Text_Exp;
    [SerializeField] private TMP_Text Text_Diamond;

    private PlayerResourceViewModel _playerResourceViewModel;

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        if (_playerResourceViewModel != null)
        {
            Bind();
            UpdateView();
        }
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Init()
    {
        if (Text_Gold == null)
        {
            Debug.LogError("[PlayerResourceView] Text_Gold가 연결되지 않았습니다.");
            return;
        }

        if (Text_Exp == null)
        {
            Debug.LogError("[PlayerResourceView] Text_Exp가 연결되지 않았습니다.");
            return;
        }

        if (Text_Diamond == null)
        {
            Debug.LogError("[PlayerResourceView] Text_Diamond가 연결되지 않았습니다.");
            return;
        }

        if (NetworkManager.Instance == null || NetworkManager.Instance.PlayerResourceService == null)
        {
            Debug.LogError("[PlayerResourceView] PlayerResourceService가 없습니다.");
            return;
        }

        _playerResourceViewModel = NetworkManager.Instance.PlayerResourceService.GetPlayerResourceViewModel();

        Bind();
        UpdateView();
    }

    private void Bind()
    {
        if (_playerResourceViewModel == null)
        {
            return;
        }

        _playerResourceViewModel.PropertyChanged -= OnPropertyChanged;
        _playerResourceViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void Unbind()
    {
        if (_playerResourceViewModel == null)
        {
            return;
        }

        _playerResourceViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(PlayerResourceViewModel.Gold) || eventArgs.PropertyName == nameof(PlayerResourceViewModel.Exp) || eventArgs.PropertyName == nameof(PlayerResourceViewModel.Diamond))
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        if (_playerResourceViewModel == null || Text_Gold == null || Text_Exp == null || Text_Diamond == null)
        {
            return;
        }

        Text_Gold.text = _playerResourceViewModel.Gold.ToString("N0");
        Text_Exp.text = _playerResourceViewModel.Exp.ToString("N0");
        Text_Diamond.text = _playerResourceViewModel.Diamond.ToString("N0");

    }
}
