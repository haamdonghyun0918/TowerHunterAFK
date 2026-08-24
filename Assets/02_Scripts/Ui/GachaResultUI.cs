using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultUI : UiBase
{
    [SerializeField] private Transform Root_Card;
    [SerializeField] private CardController Prefab_Card;
    [SerializeField] public Button Button_CloseScreen;
    [SerializeField] private Button Button_Close;
    

    private CardController _currentCard;
    private bool _isAnimPlaying = false;

    private void OnEnable()
    {
        Button_CloseScreen.gameObject.SetActive(false);
        BindButtons();
    }

    private void OnDisable()
    {
        UnbindButtons();
        Button_CloseScreen.gameObject.SetActive(false);
    }
    private void BindButtons()
    {
        Button_CloseScreen.onClick.RemoveListener(OnClick_CloseUi);
        Button_CloseScreen.onClick.AddListener(OnClick_CloseUi);

        Button_Close.onClick.RemoveListener(OnClick_CloseUi);
        Button_Close.onClick.AddListener(OnClick_CloseUi);
    }

    private void UnbindButtons()
    {
        Button_CloseScreen.onClick.RemoveListener(OnClick_CloseUi);

        Button_Close.onClick.RemoveListener(OnClick_CloseUi);
    }

    private void OnClick_CloseUi()
    {
        Debug.Log("창닫기");
        UiManager.Instance.CloseUi<GachaResultUI>();
    }

    public async UniTask SetSingleGachaResult(CharacterData character)
    {
        if (_isAnimPlaying == true) return;

        _isAnimPlaying = true;

        try
        {
            ClearCards();

            _currentCard = Instantiate(Prefab_Card, Root_Card);

            await _currentCard.SetCard(character);

            await _currentCard.OpenCard();
        }

        finally
        {
            _isAnimPlaying = false;
        }
    }

    public async UniTask SetMultipleGachaResult(List<CharacterData> characters)
    {
        if (_isAnimPlaying == true) return;

        _isAnimPlaying = true;

        try
        {
            ClearCards();

            List<CardController> cards = new List<CardController>();
            List<UniTask> loadTasks = new List<UniTask>();

            foreach (CharacterData character in characters)
            {
                CardController card = Instantiate(Prefab_Card, Root_Card);

                cards.Add(card);

                loadTasks.Add(card.SetCard(character));
            }

            await UniTask.WhenAll(loadTasks);

            List<UniTask> animationTasks = new List<UniTask>();

            foreach (CardController card in cards)
            {
                animationTasks.Add(card.OpenCard());

                await UniTask.Delay(120);
            }

            await UniTask.WhenAll(animationTasks);
        }

        finally
        {
            _isAnimPlaying = false;
        }
    }

    private void ClearCards()
    {
        foreach (Transform currentCard in Root_Card)
        {
            Destroy(currentCard.gameObject);
        }
    }
}
