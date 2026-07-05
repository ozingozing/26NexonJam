using UnityEngine;
using UnityEngine.EventSystems;

public class TowerBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;

    [Header("Build")]
    [SerializeField] private LayerMask buildGroundMask;
    [SerializeField] private float towerYOffset = 0.5f;

    [Header("Current Selection")]
    [SerializeField] private TowerData selectedTower;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryBuildTower();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ClearSelection();
        }
    }

    public void SelectTower(TowerData towerData)
    {
        selectedTower = towerData;

        Debug.Log($"선택된 타워: {towerData.towerName}");
    }

    public void ClearSelection()
    {
        selectedTower = null;
        Debug.Log("타워 선택 해제");
    }

    private void TryBuildTower()
    {
		if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
		{
			return;
		}

		// UI 버튼을 클릭했을 때 맵에도 타워가 설치되는 문제 방지
		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (selectedTower == null)
        {
            return;
        }

        if (selectedTower.towerPrefab == null)
        {
            Debug.LogWarning("선택된 타워의 Prefab이 없습니다.");
            return;
        }


        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 1000f, buildGroundMask))
        {
            return;
        }

        Vector3 hitPoint = hit.point;

        if (!grid.CanBuildTower(hitPoint))
        {
            Debug.Log("이 위치에는 포탑을 설치할 수 없습니다.");
            return;
        }
		if (PlayerGold.Instance == null)
		{
			Debug.LogWarning("PlayerGold가 씬에 없습니다.");
			return;
		}

		if (!PlayerGold.Instance.CanSpend(selectedTower.cost))
		{
			Debug.Log("골드가 부족합니다.");
			return;
		}

		Vector3 buildPosition = grid.GetBuildPosition(hitPoint);
        buildPosition.y += towerYOffset;

		GameObject buildObject = 
            Instantiate(selectedTower.towerPrefab,
                        buildPosition + Vector3.down,
                        Quaternion.identity);

        BuildableWall wall = buildObject.GetComponent<BuildableWall>();

        if (wall != null)
        {
            wall.Init(grid);
        }
        else
        {
            grid.SetTowerOnNode(hitPoint, true);
        }

		PlayerGold.Instance.TrySpendGold(selectedTower.cost);
	}
}