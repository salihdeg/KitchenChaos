using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private Button _soundEffectsButton;
    [SerializeField] private Button _musicButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _moveUpButton;
    [SerializeField] private Button _moveDownButton;
    [SerializeField] private Button _moveLeftButton;
    [SerializeField] private Button _moveRightButton;
    [SerializeField] private Button _interactButton;
    [SerializeField] private Button _interactAlteranteButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _gamepadInteractButton;
    [SerializeField] private Button _gamepadInteractAlteranteButton;
    [SerializeField] private Button _gamepadPauseButton;

    [SerializeField] private TextMeshProUGUI _soundEffectsText;
    [SerializeField] private TextMeshProUGUI _musicText;

    [SerializeField] private TextMeshProUGUI _moveUpText;
    [SerializeField] private TextMeshProUGUI _moveDownText;
    [SerializeField] private TextMeshProUGUI _moveLeftText;
    [SerializeField] private TextMeshProUGUI _moveRightText;
    [SerializeField] private TextMeshProUGUI _interactText;
    [SerializeField] private TextMeshProUGUI _interactAlteranteText;
    [SerializeField] private TextMeshProUGUI _pauseText;
    [SerializeField] private TextMeshProUGUI _gamepadInteractText;
    [SerializeField] private TextMeshProUGUI _gamepadInteractAlteranteText;
    [SerializeField] private TextMeshProUGUI _gamepadPauseText;
    [SerializeField] private Transform _pressToRebindKeyTransform;

    private Action _onCloseButtonAction;

    private void Awake()
    {
        Instance = this;

        _soundEffectsButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        _musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        _closeButton.onClick.AddListener(() =>
        {
            Hide();
            _onCloseButtonAction();
        });

        _moveUpButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Up); });
        _moveDownButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Down); });
        _moveLeftButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Left); });
        _moveRightButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Right); });
        _interactButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Interact); });
        _interactAlteranteButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.InteractAlternate); });
        _pauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Pause); });
        // GAMEPAD INPUTS
        _gamepadInteractButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_Interact); });
        _gamepadInteractAlteranteButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_InteractAlternate); });
        _gamepadPauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_Pause); });
    }

    private void Start()
    {
        GameManager.Instance.OnLocalGameUnPaused += GameManager_OnGameUnPaused;
        UpdateVisual();
        HidePressToRebindKey();
        Hide();
    }

    private void GameManager_OnGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual()
    {
        _soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f).ToString();
        _musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f).ToString();

        _moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        _moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        _moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        _moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);

        _interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        _interactAlteranteText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
        _pauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);

        // GAMEPAD INPUTS
        _gamepadInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
        _gamepadInteractAlteranteText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_InteractAlternate);
        _gamepadPauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Pause);
    }

    public void Show(Action onCloseButtonAction)
    {
        _onCloseButtonAction = onCloseButtonAction;
        gameObject.SetActive(true);

        _soundEffectsButton.Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ShowPressToRebindKey()
    {
        _pressToRebindKeyTransform.gameObject.SetActive(true);
    }

    private void HidePressToRebindKey()
    {
        _pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding, () =>
        {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }
}
