using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    private const string NUMBER_POPUP = "NumberPopup";

    [SerializeField] private TextMeshProUGUI _countdownText;

    private Animator _animator;
    private int _previousCountDownNumber;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void Update()
    {
        int countDownNumber = Mathf.CeilToInt(GameManager.Instance.GetCountdownToStartTimer());
        _countdownText.text = countDownNumber.ToString();

        if (countDownNumber != _previousCountDownNumber)
        {
            _previousCountDownNumber = countDownNumber;
            _animator.SetTrigger(NUMBER_POPUP);
            SoundManager.Instance.PlayCountDownSound();
        }
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
