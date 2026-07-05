using UnityEngine;
using System;

public class PlayerGold : MonoBehaviour
{
	public static PlayerGold Instance { get; private set; }

	[Header("Gold")]
	[SerializeField] private int startGold = 100;
	[SerializeField] private int currentGold;

	public int CurrentGold => currentGold;

	public event Action<int> OnGoldChanged;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		currentGold = startGold;
	}

	private void Start()
	{
		NotifyGoldChanged();
	}

	public void AddGold(int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		currentGold += amount;
		NotifyGoldChanged();
	}

	public bool CanSpend(int amount)
	{
		return currentGold >= amount;
	}

	public bool TrySpendGold(int amount)
	{
		if (amount < 0)
		{
			return false;
		}

		if (!CanSpend(amount))
		{
			return false;
		}

		currentGold -= amount;
		NotifyGoldChanged();

		return true;
	}

	private void NotifyGoldChanged()
	{
		OnGoldChanged?.Invoke(currentGold);
	}
}