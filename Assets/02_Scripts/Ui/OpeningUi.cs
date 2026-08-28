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

    [Header("Skip Controls")]
    [SerializeField] private GameObject _leftKeyGuide;
    [SerializeField] private GameObject _rightKeyGuide;
    [SerializeField] private Image _skipRingBackspace;
    [SerializeField] private Image _skipRingSpace;

    private readonly float _longPressTime = 1.5f;
    private readonly float _holdDelay = 0.3f;
    private readonly float _cooldownTime = 1.0f;

    [Header("GuildName Input UI")]
    [SerializeField] private GameObject _inputGroup;
    [SerializeField] private TMP_InputField _inputGuildName;
    [SerializeField] private UiButton _buttonConfirm;

    private bool _isNameConfirmed = false;
    private int _currentCutIndex = 0;

    public async UniTask OpeningScene()
    {
        CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();

        if (_comicCuts == null || _comicCuts.Length == 0)
        {
            Debug.LogError("컷씬이 존재하지 않습니다.");
            return;
        }

        _currentCutIndex = 0;
        _isNameConfirmed = false;

        if (_buttonConfirm != null)
        {
            _buttonConfirm.BindOnClickButtonEvent(OnClickConfirm);
        }

        UpdateCutSceneState();

        float cooldownTimer = 0f;
        float spaceHoldTimer = 0f;
        float backspaceHoldTimer = 0f;

        while (!_isNameConfirmed)
        {
            if (destroyToken.IsCancellationRequested)
            {
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                spaceHoldTimer = 0f;
                backspaceHoldTimer = 0f;
                UpdateRingUI(_skipRingBackspace, 0f);
                UpdateRingUI(_skipRingSpace, 0f);
                await UniTask.Yield(PlayerLoopTiming.Update, destroyToken);
                continue;
            }

            bool isTyping = (_inputGroup != null && _inputGroup.activeSelf && _inputGuildName != null && _inputGuildName.isFocused);

            if (_currentCutIndex > 0)
            {
                if (Input.GetKey(KeyCode.Backspace) && !isTyping)
                {
                    backspaceHoldTimer += Time.deltaTime;

                    if (backspaceHoldTimer >= _longPressTime)
                    {
                        _currentCutIndex = 0;
                        UpdateCutSceneState();
                        cooldownTimer = _cooldownTime;
                        backspaceHoldTimer = 0f;
                    }
                }

                else
                {
                    if (Input.GetKeyUp(KeyCode.Backspace) && isTyping == false)
                    {
                        if (backspaceHoldTimer > 0f && backspaceHoldTimer <= _holdDelay)
                        {
                            _currentCutIndex--;
                            UpdateCutSceneState();
                            cooldownTimer = _cooldownTime;
                            backspaceHoldTimer = 0f;
                        }
                    }

                    if (backspaceHoldTimer > 0f)
                    {
                        backspaceHoldTimer -= Time.deltaTime;
                        if (backspaceHoldTimer < 0f)
                        {
                            backspaceHoldTimer = 0f;
                        }
                    }
                }
            }

            else
            {
                backspaceHoldTimer = 0f;
            }

            if (_currentCutIndex < _comicCuts.Length - 1 && cooldownTimer <= 0f)
            {
                if (Input.GetKey(KeyCode.Space) && isTyping == false)
                {
                    spaceHoldTimer += Time.deltaTime;
                    if (spaceHoldTimer >= _longPressTime)
                    {
                        _currentCutIndex = _comicCuts.Length - 1;
                        UpdateCutSceneState();
                        cooldownTimer = _cooldownTime;
                        spaceHoldTimer = 0f;
                    }
                }

                else
                {
                    if (Input.GetKeyUp(KeyCode.Space) && isTyping == false)
                    {
                        if (spaceHoldTimer > 0f && spaceHoldTimer <= _holdDelay)
                        {
                            _currentCutIndex++;
                            UpdateCutSceneState();
                            cooldownTimer = _cooldownTime;
                            spaceHoldTimer = 0f;
                        }
                    }

                    if (spaceHoldTimer > 0f)
                    {
                        spaceHoldTimer -= Time.deltaTime;
                        if (spaceHoldTimer < 0f)
                        {
                            spaceHoldTimer = 0f;
                        }
                    }
                }
            }

            else
            {
                spaceHoldTimer = 0f;
            }

            UpdateRingUI(_skipRingBackspace, backspaceHoldTimer);
            UpdateRingUI(_skipRingSpace, spaceHoldTimer);

            await UniTask.Yield(PlayerLoopTiming.Update, destroyToken);
        }

        if (_buttonConfirm != null)
        {
            _buttonConfirm.UnBindOnClickButtonEvent(OnClickConfirm);

        }
    }

    private void UpdateCutSceneState()
    {
        if (_screenImage != null && _comicCuts != null && _currentCutIndex < _comicCuts.Length)
        {
            _screenImage.sprite = _comicCuts[_currentCutIndex];
        }

        bool isFirstCut = (_currentCutIndex == 0);
        bool isLastCut = (_currentCutIndex == _comicCuts.Length - 1);

        if (_leftKeyGuide != null)
        {
            _leftKeyGuide.SetActive(isFirstCut == false);
        }

        if (_rightKeyGuide != null)
        {
            _rightKeyGuide.SetActive(isLastCut == false);
        }

        if (_inputGroup != null)
        {
            _inputGroup.SetActive(isLastCut);
        }
    }

    private void UpdateRingUI(Image ring, float holdTimer)
    {
        if (holdTimer <= _holdDelay)
        {
            ring.fillAmount = 0f;
            ring.gameObject.SetActive(false);
        }

        else
        {
            ring.gameObject.SetActive(true);
            float fillDuration = _longPressTime - _holdDelay;
            float currentFillTime = holdTimer - _holdDelay;
            ring.fillAmount = Mathf.Clamp01(currentFillTime / fillDuration);
        }
    }

    private void OnClickConfirm()
    {
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