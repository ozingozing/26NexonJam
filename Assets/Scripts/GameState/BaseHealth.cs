using UnityEngine;
using System;

public class BaseHealth : MonoBehaviour
{
	[Header("Health")]
	[SerializeField] private int maxHp = 20;
	[SerializeField] private int currentHp;

	public int CurrentHp => currentHp;
	public int MaxHp => maxHp;

	public event Action<int, int> OnHealthChanged;
	public event Action OnBaseDestroyed;

	private void Awake()
	{
		currentHp = maxHp;
	}

	private void Start()
	{
		NotifyHealthChanged();
	}

	public void TakeDamage(int damage)
	{
		if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
		{
			return;
		}

		currentHp -= damage;
		currentHp = Mathf.Clamp(currentHp, 0, maxHp);

		NotifyHealthChanged();

		if (currentHp <= 0)
		{
			Die();
		}
	}

	public void Heal(int amount)
	{
		if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
		{
			return;
		}

		currentHp += amount;
		currentHp = Mathf.Clamp(currentHp, 0, maxHp);

		NotifyHealthChanged();
	}

	private void NotifyHealthChanged()
	{
		OnHealthChanged?.Invoke(currentHp, maxHp);
	}

	private void Die()
	{
		OnBaseDestroyed?.Invoke();

		if (GameManager.Instance != null)
		{
			GameManager.Instance.GameOver();
		}
	}
}