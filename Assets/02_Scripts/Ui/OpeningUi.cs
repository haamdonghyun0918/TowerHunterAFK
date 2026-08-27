using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpeningUi : UiBase
{
    [Header("CutScene")]
    [SerializeField] private Sprite[] _comicCuts;
    [SerializeField] private Image _screenImage;

    [Header("Skip")]
    [SerializeField] private Image _skipRing;
    [SerializeField] private float _skipTimer = 1.5f;

    [Header("GuildName")]
    [SerializeField] private TMP_Text _textGuildName;
    [SerializeField] private UiButton _buttonConfirm;

    public async UniTask OpeningScene()
    {
        CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();

        if (_skipRing != null)
        {
            _skipRing.fillAmount = 0f;
            _skipRing.gameObject.SetActive(false);
        }

        if (_comicCuts == null || _comicCuts.Length == 0)
        {
            Debug.LogError("컷씬이 존재하지 않습니다.");
            return;
        }

        if (_screenImage != null)
        {
            _screenImage.gameObject.SetActive(true);
        }

        for (int i = 0; i < _comicCuts.Length; i++)
        {
            if (destroyToken.IsCancellationRequested) return;

            if (_screenImage != null)
            {
                _screenImage.sprite = _comicCuts[i];
            }

            if (i == _comicCuts.Length - 1)
            {
                break;
            }

            if (i > 0)
            {
                await UniTask.Delay(1000, cancellationToken: destroyToken);
            }

            float holdTimer = 0f;
            if (_skipRing != null) _skipRing.gameObject.SetActive(true);

            while (holdTimer < _skipTimer)
            {
                if (destroyToken.IsCancellationRequested)
                {
                    return;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    holdTimer += Time.deltaTime;
                }

                else
                {
                    if (holdTimer > 0f)
                    {
                        holdTimer -= Time.deltaTime;
                        if (holdTimer < 0f) holdTimer = 0f;
                    }
                }

                if (_skipRing != null)
                {
                    _skipRing.fillAmount = holdTimer / _skipTimer;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, destroyToken);
            }

            if (_skipRing != null)
            {
                _skipRing.fillAmount = 0f;
                _skipRing.gameObject.SetActive(false);
            }
        }

        if (_skipRing != null) _skipRing.gameObject.SetActive(false);
    }
}