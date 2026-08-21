using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class GachaResultUI : UiBase
{
    [SerializeField] private Transform Root_Card;
    [SerializeField] private CardController Prefab_Card;

    private CardController _currentCard;
    private bool _isAnimPlaying = false;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        CloseUI();
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

    private void CloseUI()
    {
        UiManager.Instance.CloseUi<GachaResultUI>();
    }
}
