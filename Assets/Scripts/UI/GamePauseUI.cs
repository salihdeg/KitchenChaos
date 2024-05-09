using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _optionsButton;

    private void Awake()
    {
        _resumeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.TogglePauseGame();
        });

        _mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        _optionsButton.onClick.AddListener(() =>
        {
            Hide();
            OptionsUI.Instance.Show(Show);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnLocalGamePaused += GameManager_OnLocalGamePaused;
        GameManager.Instance.OnLocalGameUnPaused += Instance_OnLocalGameUnPaused;

        Hide();
    }

    private void GameManager_OnLocalGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Instance_OnLocalGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        _resumeButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
