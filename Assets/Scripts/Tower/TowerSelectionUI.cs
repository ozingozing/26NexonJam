using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class TowerButtonUI
    {
        public TowerType towerType;
        public Button button;
        public TMP_Text buttonText;
        public Image buttonIcon;
    }
	[Header("Gold UI")]
	[SerializeField] private TMP_Text goldText;

	[Header("References")]
    [SerializeField] private TowerBuilder towerBuilder;

    [Header("Tower Data")]
    [SerializeField] private TowerData[] towers;

    [Header("Tower Buttons")]
    [SerializeField] private TowerButtonUI[] towerButtons;

    [Header("Selected Tower UI")]
    [SerializeField] private TMP_Text selectedTowerText;

    private TowerType? selectedTowerType = null;

    private void Start()
    {
        UpdateAllButtonUI();
        UpdateSelectedTowerUI(null);
        BindButtonEvents();

		if (PlayerGold.Instance != null)
		{
			UpdateGoldUI(PlayerGold.Instance.CurrentGold);
			UpdateTowerButtonInteractable(PlayerGold.Instance.CurrentGold);
		}
	}

	private void OnEnable()
	{
		if (PlayerGold.Instance != null)
		{
			PlayerGold.Instance.OnGoldChanged += UpdateGoldUI;
		}
	}

	private void OnDisable()
	{
		if (PlayerGold.Instance != null)
		{
			PlayerGold.Instance.OnGoldChanged -= UpdateGoldUI;
		}
	}

	private void UpdateGoldUI(int gold)
	{
		if (goldText != null)
		{
			goldText.text = $"Gold: {gold}";
		}

		UpdateTowerButtonInteractable(gold);
	}

	private void UpdateTowerButtonInteractable(int currentGold)
	{
		for (int i = 0; i < towerButtons.Length; i++)
		{
			TowerButtonUI buttonUI = towerButtons[i];
			TowerData towerData = GetTowerData(buttonUI.towerType);

			if (towerData == null || buttonUI.button == null)
			{
				continue;
			}

			buttonUI.button.interactable = currentGold >= towerData.cost;
		}
	}

	private void BindButtonEvents()
    {
        for (int i = 0; i < towerButtons.Length; i++)
        {
            TowerButtonUI buttonUI = towerButtons[i];

            if (buttonUI.button == null)
            {
                continue;
            }

            TowerType type = buttonUI.towerType;

            buttonUI.button.onClick.RemoveAllListeners();
            buttonUI.button.onClick.AddListener(() =>
            {
                SelectTower(type);
            });
        }
    }

    private void UpdateAllButtonUI()
    {
        for (int i = 0; i < towerButtons.Length; i++)
        {
            TowerButtonUI buttonUI = towerButtons[i];
            TowerData towerData = GetTowerData(buttonUI.towerType);

            if (towerData == null)
            {
                continue;
            }

            UpdateButtonUI(buttonUI, towerData);
        }
    }

    private void UpdateButtonUI(TowerButtonUI buttonUI, TowerData towerData)
    {
        if (buttonUI.buttonText != null)
        {
            buttonUI.buttonText.text = $"{towerData.towerName}\n{towerData.cost}G";
        }

        if (buttonUI.buttonIcon != null)
        {
            buttonUI.buttonIcon.sprite = towerData.icon;
            buttonUI.buttonIcon.enabled = towerData.icon != null;
        }
    }

    public void SelectBasicTower()
    {
        SelectTower(TowerType.Basic);
    }

    public void SelectMissileTower()
    {
        SelectTower(TowerType.Missile);
    }

    public void SelectSlowTower()
    {
        SelectTower(TowerType.Slow);
    }

    public void SelectWall()
    {
        SelectTower(TowerType.Wall);
    }

    public void SelectTower(TowerType towerType)
    {
        TowerData towerData = GetTowerData(towerType);

        if (towerData == null)
        {
            Debug.LogWarning($"{towerType} 타워 데이터를 찾을 수 없습니다.");
            return;
        }

        selectedTowerType = towerType;

        towerBuilder.SelectTower(towerData);
        UpdateSelectedTowerUI(towerData);
    }

    public void CancelSelection()
    {
        selectedTowerType = null;

        towerBuilder.ClearSelection();
        UpdateSelectedTowerUI(null);
    }

    private void UpdateSelectedTowerUI(TowerData towerData)
    {
        if (towerData == null)
        {
            if (selectedTowerText != null)
            {
                selectedTowerText.text = "Selected Tower : None";
            }

            return;
        }

        if (selectedTowerText != null)
        {
            selectedTowerText.text = $"{towerData.towerName} / {towerData.cost}G";
        }
    }

    private TowerData GetTowerData(TowerType towerType)
    {
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i].towerType == towerType)
            {
                return towers[i];
            }
        }

        return null;
    }
}