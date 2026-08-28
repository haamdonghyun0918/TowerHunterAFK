using Cysharp.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpeningUi : UiBase
{
    public static event Action OnGuildNameSet;

    [Header("CutScene")]
    [SerializeField] private Sprite[] _comicCuts;
    [SerializeField] private Image _screenImage;

    [Header("Skip")]
    [SerializeField] private Image _skipRing;
    [SerializeField] private float _skipTimer = 1f;

    [Header("GuildName Input UI")]
    [SerializeField] private GameObject _inputGroup;
    [SerializeField] private TMP_InputField _inputGuildName;
    [SerializeField] private UiButton _buttonConfirm;

    private bool _isNameConfirmed = false;

    public async UniTask OpeningScene()
    {
        CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();

        if (_inputGroup != null)
        {
            _inputGroup.SetActive(false);
        }

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
            if (destroyToken.IsCancellationRequested)
            {
                return;
            }

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

            if (_skipRing != null)
            {
                _skipRing.gameObject.SetActive(true);
            }

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

                        if (holdTimer < 0f)
                        {
                            holdTimer = 0f;
                        }
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

        if (_skipRing != null)
        {
            _skipRing.gameObject.SetActive(false);
        }

        if (_inputGroup != null)
        {
            _inputGroup.SetActive(true);
        }

        _buttonConfirm.BindOnClickButtonEvent(OnClickConfirm);

        while (_isNameConfirmed == false)
        {
            if (destroyToken.IsCancellationRequested)
            {
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, destroyToken);
        }

        _buttonConfirm.UnBindOnClickButtonEvent(OnClickConfirm);
    }

    private void OnClickConfirm()
    {
        if (_inputGuildName == null)
        {
            return;
        }

        string inputName = _inputGuildName.text.Trim();

        if (Regex.IsMatch(inputName, @"^[a-zA-Z0-9가-힣]{2,10}$"))
        {
            SaveManager.Instance.SaveGuildName(inputName);
            OnGuildNameSet?.Invoke();
            _isNameConfirmed = true;
        }

        else
        {
            Debug.LogWarning("길드 이름은 공백이나 특수문자 없이 2~10글자여야 합니다.");
        }
    }
}