using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseHealthUI : MonoBehaviour
{
	[Header("Target")]
	[SerializeField] private BaseHealth baseHealth;

	[Header("UI")]
	[SerializeField] private Image hpFillImage;

	[Header("Animation")]
	[SerializeField] private float fillSmoothSpeed = 10f;

	private float targetFillAmount = 1f;

	private void Awake()
	{
		if (baseHealth == null)
		{
			baseHealth = FindObjectOfType<BaseHealth>();
		}
	}

	private void OnEnable()
	{
		if (baseHealth != null)
		{
			baseHealth.OnHealthChanged += UpdateHealthUI;
		}
	}

	private void OnDisable()
	{
		if (baseHealth != null)
		{
			baseHealth.OnHealthChanged -= UpdateHealthUI;
		}
	}

	private void Start()
	{
		if (baseHealth != null)
		{
			UpdateHealthUI(baseHealth.CurrentHp, baseHealth.MaxHp);

			if (hpFillImage != null)
			{
				hpFillImage.fillAmount = targetFillAmount;
			}
		}
	}

	private void Update()
	{
		if (hpFillImage == null)
		{
			return;
		}

		hpFillImage.fillAmount = Mathf.Lerp(
			hpFillImage.fillAmount,
			targetFillAmount,
			fillSmoothSpeed * Time.unscaledDeltaTime
		);
	}

	private void UpdateHealthUI(int currentHp, int maxHp)
	{
		targetFillAmount = (float)currentHp / maxHp;
	}
}