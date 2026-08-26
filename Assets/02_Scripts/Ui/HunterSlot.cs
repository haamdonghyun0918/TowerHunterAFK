using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class HunterSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text _hunterName;
    [SerializeField] private TMP_Text _textLevel;
    [SerializeField] private TMP_Text _textEnhanceRank;
    [SerializeField] private UiButton _hunterSlotButton;
    [SerializeField] private Image _hunterIcon;
    [SerializeField] private Image _progressRing;

    [SerializeField] private GameObject _weaponCannes;
    [SerializeField] private GameObject _armorCannes;
    [SerializeField] private GameObject _accessoriesCannes;

    private string _uniqueId;
    private Action<string> _onClickSlot;
    private Action<string> _onLongPressSlot;

    private CancellationTokenSource _cts;
    private CancellationTokenSource _longPressCts;

    private bool _isPointerDown = false;
    private bool _isLongPressTriggered = false;

    private const long LevelUpExp = 2000;

    private bool _isClickCanceled = false;
    private const float TapThreshold = 0.2f;
    private const float LongPressDuration = 0.6f;

    public void SetUp(CharacterData data, string uniqueId, Action<string> onClick = null, Action<string> onLongPress = null)
    {
        _uniqueId = uniqueId;
        _onClickSlot = onClick;
        _onLongPressSlot = onLongPress;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        BindEquipmentEvent();
        UpdateEquipmentCannes();

        if (data != null)
        {
            _hunterName.text = data.Name;

            UpdateSlotInfo(uniqueId);

            if (string.IsNullOrEmpty(data.IconPath) == false)
            {
                _hunterIcon.sprite = null;
                _hunterIcon.gameObject.SetActive(false);
                LoadIconAsync(data.IconPath, _cts.Token).Forget();
            }

            else
            {
                _hunterIcon.sprite = null;
                _hunterIcon.gameObject.SetActive(false);
            }
        }

        else
        {
            _hunterName.text = "비어있음";

            if (_textLevel != null)
            {
                _textLevel.text = "";
            }

            if (_textEnhanceRank != null)
            {
                _textEnhanceRank.text = "";
            }

            if (_hunterIcon != null)
            {
                _hunterIcon.sprite = null;
                _hunterIcon.gameObject.SetActive(false);
            }
        }

        if (_hunterSlotButton != null)
        {
            _hunterSlotButton.UnBindOnClickButtonEvent(OnClickSlot);
            _hunterSlotButton.BindOnClickButtonEvent(OnClickSlot);
        }

        if (_progressRing != null)
        {
            _progressRing.fillAmount = 0f;
            _progressRing.gameObject.SetActive(false);
        }
    }

    private void BindEquipmentEvent()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[HunterSlot] NetworkManager.Instance가 없습니다.");
            return;
        }

        if (NetworkManager.Instance.EquipmentService == null)
        {
            Debug.LogError("[HunterSlot] EquipmentService가 없습니다.");
            return;
        }

        NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged -= OnHunterEquipmentChanged;
        NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged += OnHunterEquipmentChanged;
    }

    private void UnbindEquipmentEvent()
    {
        if (NetworkManager.Instance != null)
        {
            if (NetworkManager.Instance.EquipmentService != null)
            {
                NetworkManager.Instance.EquipmentService.CharacterEquipmentChanged -= OnHunterEquipmentChanged;
            }
        }
    }

    private void OnHunterEquipmentChanged(string changedHunterUid)
    {
        if (_uniqueId == changedHunterUid)
        {
            UpdateEquipmentCannes();
        }
    }

    private void UpdateEquipmentCannes()
    {
        if (_weaponCannes == null)
        {
            Debug.LogError("[HunterSlot] _weaponCannes가 연결되지 않았습니다.");
            return;
        }

        if (_armorCannes == null)
        {
            Debug.LogError("[HunterSlot] _armorCannes가 연결되지 않았습니다.");
            return;
        }

        if (_accessoriesCannes == null)
        {
            Debug.LogError("[HunterSlot] _accessoriesCannes가 연결되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(_uniqueId) == true)
        {
            _weaponCannes.SetActive(false);
            _armorCannes.SetActive(false);
            _accessoriesCannes.SetActive(false);
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[HunterSlot] SaveManager.Instance가 없습니다.");
            return;
        }

        CharacterSaveData mySaveData;
        bool isExist = SaveManager.Instance.CharacterDict.TryGetValue(_uniqueId, out mySaveData);

        if (isExist == false)
        {
            Debug.LogError($"[HunterSlot] 세이브 데이터에서 헌터를 찾을 수 없습니다. UID: {_uniqueId}");
            return;
        }

        bool hasWeapon = string.IsNullOrEmpty(mySaveData.EquippedWeaponUid) == false;
        bool hasArmor = string.IsNullOrEmpty(mySaveData.EquippedArmorUid) == false;
        bool hasAccessory = string.IsNullOrEmpty(mySaveData.EquippedAccessoryUid) == false;

        _weaponCannes.SetActive(hasWeapon);
        _armorCannes.SetActive(hasArmor);
        _accessoriesCannes.SetActive(hasAccessory);
    }

    private void UpdateSlotInfo(string uniqueId)
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CharacterDict.TryGetValue(uniqueId, out var saveData) == false)
        {
            return;
        }

        if (_textLevel != null)
        {
            int currentLevel = 1 + (int)(saveData.Exp / LevelUpExp);
            _textLevel.text = $"Lv.{currentLevel}";
        }

        if (_textEnhanceRank != null)
        {
            int rank = saveData.Rank;

            if (rank <= 0)
            {
                _textEnhanceRank.text = "";
            }

            else
            {
                _textEnhanceRank.text = $"+{rank}";

                if (rank == 1 || rank == 2)
                {
                    _textEnhanceRank.color = new Color32(205, 127, 50, 255);
                }

                else if (rank == 3 || rank == 4)

                {
                    _textEnhanceRank.color = new Color32(192, 192, 192, 255);
                }

                else if (rank >= 5)
                {
                    _textEnhanceRank.color = new Color32(255, 215, 0, 255);
                }
            }
        }
    }

    private async UniTaskVoid LoadIconAsync(string iconPath, CancellationToken token)
    {
        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(iconPath);
       
        if (token.IsCancellationRequested)
        {
            return;
        }

        if (loadedSprite != null)
        {
            _hunterIcon.sprite = loadedSprite;
            _hunterIcon.gameObject.SetActive(true);
        }

        else
        {
            Debug.LogWarning($"[HunterSlot] 아이콘 로드 실패: {iconPath}");
        }
    }

    private void OnClickSlot()
    {
        if (_isLongPressTriggered || _isClickCanceled)
        {
            return;
        }

        _onClickSlot?.Invoke(_uniqueId);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _longPressCts?.Cancel();
        _longPressCts?.Dispose();

        UnbindEquipmentEvent();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _isLongPressTriggered = false;
        _isClickCanceled = false;
        
        if (_onLongPressSlot == null)
        {
            return;
        }
        _longPressCts?.Cancel();
        _longPressCts?.Dispose();
        _longPressCts = new CancellationTokenSource();

        CheckLongPress(_longPressCts.Token).Forget();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        _longPressCts?.Cancel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerDown = false;
        _longPressCts?.Cancel();
    }

    private async UniTaskVoid CheckLongPress(CancellationToken token)
    {
        if (_progressRing == null)
        {
            Debug.Log("원형 이미지가 존재 하지 않습니다.");
            return;
        }

        float elapsedTime = 0f;
        _progressRing.fillAmount = 0f;
        _progressRing.gameObject.SetActive(false);

        try
        {
            while (elapsedTime < LongPressDuration)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= TapThreshold)
                {
                    _isClickCanceled = true;
                    _progressRing.gameObject.SetActive(true);

                    float fillRatio = (elapsedTime - TapThreshold) / (LongPressDuration - TapThreshold);
                    _progressRing.fillAmount = fillRatio;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }

            if (_isPointerDown)
            {
                _progressRing.fillAmount = 1f;
                _isLongPressTriggered = true;
                _onLongPressSlot?.Invoke(_uniqueId);
            }
        }

        catch
        {
        }

        finally
        {
            _progressRing.fillAmount = 0f;
            _progressRing.gameObject.SetActive(false);
        }
    }
}