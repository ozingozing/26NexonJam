using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Game State")]
	public GameState currentState = GameState.Ready;

	[Header("Optional UI")]
	[SerializeField] private GameObject gameOverPanel;

	public bool IsGameOver => currentState == GameState.GameOver;
	public bool IsPlaying => currentState == GameState.Playing;

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
		if (currentState == GameState.GameOver)
		{
			return;
		}

		currentState = GameState.GameOver;

		Debug.Log("Game Over");

		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(true);
		}

		// 게임을 완전히 멈추고 싶으면 사용
		Time.timeScale = 0f;
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}