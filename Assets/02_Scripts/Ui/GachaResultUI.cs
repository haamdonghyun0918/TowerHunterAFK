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

    public async UniTask SetGachaResult(CharacterData character)
    {
        if (_isAnimPlaying == true) return;

        _isAnimPlaying = true;

        try
        {
            ClearCards();

            _currentCard = Instantiate(Prefab_Card, Root_Card);

            await _currentCard.SetCard(character);
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

            List<UniTask> cardTasks = new List<UniTask>();

            foreach (CharacterData character in characters)
            {
                _currentCard = Instantiate(Prefab_Card, Root_Card);
                cardTasks.Add(_currentCard.SetCard(character));
            }

            await UniTask.WhenAll(cardTasks);
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
