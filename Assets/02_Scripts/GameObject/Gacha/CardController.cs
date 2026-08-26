using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [SerializeField] private RectTransform _cardTransform;
    [SerializeField] private Image _glowImage;
    [SerializeField] private GameObject _cardBack;
    [SerializeField] private GameObject _cardFront;
    [SerializeField] private Image _characterCardIcon;
    [SerializeField] private TMP_Text _characterName;

    private RectTransform _glowTransform;

    [SerializeField] private Color _gradeC;
    [SerializeField] private Color _gradeB;
    [SerializeField] private Color _gradeA;
    [SerializeField] private Color _gradeS;

    private Color _gradeColor;

    private bool _isOpened;

    private void Awake()
    {
        _glowTransform = _glowImage.rectTransform;
    }

    private void OnEnable()
    {
        InitCard();
    }

    private void InitCard()
    {
        _isOpened = false;

        _cardBack.SetActive(true);
        _cardFront.gameObject.SetActive(false);

        _characterName.gameObject.SetActive(false);

        _glowTransform.localScale = Vector3.one;

        _glowImage.color = new Color(1, 1, 1, 0);

        _cardTransform.localScale = Vector3.one;
    }

    public async UniTask SetCard(CharacterData character)
    {
        SetGradeColor(character);
        SetCardFrontImage(character.Rarity);

        await SetCharacterCardIcon(character);

        _characterName.text = character.Name;
    }

    public async UniTask OpenCard()
    {
        if (_isOpened) return;

        _isOpened = true;

        DG.Tweening.Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            _glowImage.color = Color.white;
            _glowImage.DOFade(1f, 0.2f);
        });

        sequence.Join(_glowTransform.DOScale(1.1f, 0.2f));

        sequence.Append(_glowImage.DOColor(_gradeColor, 0.5f));

        sequence.Join(
            _glowTransform.DOScale(1.3f, 0.5f)
        );

        sequence.AppendCallback(() =>
        {
            _cardTransform.DOShakeRotation(
                1.5f,
                new Vector3(0f, 0f, 5f),
                20
            );

            _glowTransform
                .DOScale(1.5f, 0.25f)
                .SetLoops(6, LoopType.Yoyo);
        });

        sequence.AppendInterval(1.5f);

        sequence.Append(_cardTransform.DOScaleX(0f, 0.2f));

        sequence.AppendCallback(() =>
        {
            _cardBack.SetActive(false);
            _cardFront.gameObject.SetActive(true);
            _characterName.gameObject.SetActive(true);
        });

        sequence.Append(_cardTransform.DOScaleX(1f, 0.2f));

        sequence.Join(
            _glowTransform
                .DOScale(1f, 0.5f)
                .SetEase(Ease.OutElastic)
        );

        sequence.Join(
            _glowTransform.DOShakeRotation(
                0.5f,
                new Vector3(0f, 0f, 8f),
                20,
                90f,
                true
            )
        );

        sequence.AppendInterval(0.2f);

        sequence.Append(
            _glowImage.DOFade(0f, 0.4f)
        );

        await sequence.AsyncWaitForCompletion();
    }

    private void SetCardFrontImage(string rarity)
    {
        string imagePath = $"Card/CardFrontImage/CharacterCard_{rarity}";

        var frontCardPrefab = Resources.Load<GameObject>(imagePath);

        if (frontCardPrefab == null)
        {
            Debug.LogError($"[CardController] {imagePath}경로로 카드 Sprite를 불러올 수 없습니다.");
            return;
        }

        Instantiate(frontCardPrefab, _cardFront.transform);
    }

    private void SetGradeColor(CharacterData character)
    {
        string rarity = character.Rarity;
        switch (rarity)
        {
            case "C":
                {
                    _gradeColor = _gradeC;
                }
                break;
            case "B":
                {
                    _gradeColor = _gradeB;
                }
                break;
            case "A":
                {
                    _gradeColor = _gradeA;
                }
                break;
            case "S":
                {
                    _gradeColor = _gradeS;
                }
                break;
            default:
                {
                    return;
                }
        }
    }

    private async UniTask SetCharacterCardIcon(CharacterData character)
    {
        string iconPath = character.IconPath;

        Sprite icon = await Addressables.LoadAssetAsync<Sprite>(iconPath).Task;

        if (icon == null)
        {
            Debug.LogError($"[CardController] 캐릭터 아이콘을 불러오지 못했습니다. {iconPath}");
            return;
        }

        _characterCardIcon.sprite = icon;
    }
}
