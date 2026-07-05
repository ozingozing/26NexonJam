using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Game State")]
	public GameState currentState = GameState.Ready;

	[Header("BGM")]
	[SerializeField] private AudioSource bgmAudioSource;
	[SerializeField] private AudioClip bgmClip;
	[SerializeField] private RectTransform cdIcon;
	[SerializeField] private float cdRotateSpeed = 180f;
	[SerializeField] private bool playBgmOnStart = true;
	[SerializeField] private bool stopBgmOnGameEnd = true;

	[Header("Result UI")]
	[SerializeField] private GameObject gameOverPanel;
	[SerializeField] private TMP_Text resultText;

	public bool IsGameOver => currentState == GameState.GameOver;
	public bool IsClear => currentState == GameState.Clear;
	public bool IsPlaying => currentState == GameState.Playing;
	public bool IsGameEnded => currentState == GameState.GameOver || currentState == GameState.Clear;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		if (bgmAudioSource == null)
		{
			bgmAudioSource = GetComponent<AudioSource>();
		}
	}

	private void Start()
	{
		StartGame();
	}

	private void Update()
	{
		RotateCdIcon();
	}

	public void StartGame()
	{
		currentState = GameState.Playing;

		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(false);
		}

		Time.timeScale = 1f;

		if (playBgmOnStart)
		{
			PlayBgm();
		}
	}

	private void PlayBgm()
	{
		if (bgmAudioSource == null)
		{
			Debug.LogWarning("BGM AudioSource가 연결되어 있지 않습니다.");
			return;
		}

		if (bgmClip != null)
		{
			bgmAudioSource.clip = bgmClip;
		}

		if (bgmAudioSource.clip == null)
		{
			Debug.LogWarning("BGM AudioClip이 없습니다.");
			return;
		}

		bgmAudioSource.loop = true;

		if (!bgmAudioSource.isPlaying)
		{
			bgmAudioSource.Play();
		}
	}

	private void StopBgm()
	{
		if (bgmAudioSource == null)
		{
			return;
		}

		bgmAudioSource.Stop();
	}

	private void RotateCdIcon()
	{
		if (cdIcon == null)
		{
			return;
		}

		if (bgmAudioSource == null || !bgmAudioSource.isPlaying)
		{
			return;
		}

		cdIcon.Rotate(
			0f,
			0f,
			-cdRotateSpeed * Time.unscaledDeltaTime
		);
	}


	public void GameOver()
	{
		if (IsGameEnded)
		{
			return;
		}

		currentState = GameState.GameOver;

		ShowResultPanel("Game Over");

		if (stopBgmOnGameEnd)
		{
			StopBgm();
		}
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.StopAllSounds();
		}

		Time.timeScale = 0f;
	}

	public void GameClear()
	{
		if (IsGameEnded)
		{
			return;
		}

		currentState = GameState.Clear;

		ShowResultPanel("Clear!");

		if (stopBgmOnGameEnd)
		{
			StopBgm();
		}
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.StopAllSounds();
		}

		Time.timeScale = 0f;
	}

	private void ShowResultPanel(string message)
	{
		Debug.Log(message);

		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(true);
		}

		if (resultText != null)
		{
			resultText.text = message;
		}
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}