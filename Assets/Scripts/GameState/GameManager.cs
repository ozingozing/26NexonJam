using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Game State")]
	public GameState currentState = GameState.Ready;

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
	}

	private void Start()
	{
		StartGame();
	}

	public void StartGame()
	{
		currentState = GameState.Playing;

		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(false);
		}

		Time.timeScale = 1f;
	}

	public void GameOver()
	{
		if (IsGameEnded)
		{
			return;
		}

		currentState = GameState.GameOver;

		ShowResultPanel("Game Over");

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