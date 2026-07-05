using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
	[Serializable]
	public class SpawnUnitData
	{
		public UnitType unitType;
		public Unit unitPrefab;
		public int count = 5;
		public float spawnInterval = 1f;
	}

	[Header("References")]
	[SerializeField] private Grid grid;

	[Header("Spawn Settings")]
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private Transform target;

	[Header("Wave Units")]
	[SerializeField] private SpawnUnitData[] waveUnits;

	[Header("Auto Start")]
	[SerializeField] private bool autoStart = true;
	[SerializeField] private float startDelay = 1f;

	private readonly List<Unit> spawnedUnits = new List<Unit>();

	private Coroutine recalculateCoroutine;

	private bool isSpawnFinished = false;

	private void Awake()
	{
		if (grid == null)
		{
			grid = FindObjectOfType<Grid>();
		}
	}

	private void OnEnable()
	{
		if (grid != null)
		{
			grid.OnGridChanged += HandleGridChanged;
		}
	}

	private void OnDisable()
	{
		if (grid != null)
		{
			grid.OnGridChanged -= HandleGridChanged;
		}
	}

	private void Start()
	{
		if (autoStart)
		{
			StartCoroutine(SpawnWaveAfterDelay());
		}
	}

	private IEnumerator SpawnWaveAfterDelay()
	{
		yield return new WaitForSeconds(startDelay);
		yield return StartCoroutine(SpawnWave());
	}

	public IEnumerator SpawnWave()
	{
		isSpawnFinished = false;

		for (int i = 0; i < waveUnits.Length; i++)
		{
			SpawnUnitData spawnData = waveUnits[i];

			for (int j = 0; j < spawnData.count; j++)
			{
				if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
				{
					yield break;
				}

				SpawnUnit(spawnData);

				yield return new WaitForSeconds(spawnData.spawnInterval);
			}
		}

		isSpawnFinished = true;
		CheckClearCondition();
	}

	private void SpawnUnit(SpawnUnitData spawnData)
	{
		if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
		{
			return;
		}

		if (spawnData.unitPrefab == null)
		{
			Debug.LogWarning($"{spawnData.unitType} 타입의 unitPrefab이 비어 있습니다.");
			return;
		}

		Unit unit = Instantiate(
			spawnData.unitPrefab,
			spawnPoint.position,
			Quaternion.identity
		);

		spawnedUnits.Add(unit);
		unit.OnUnitRemoved += HandleUnitRemoved;

		unit.Init(target);
	}

	private void HandleUnitRemoved(Unit unit)
	{
		if (unit != null)
		{
			unit.OnUnitRemoved -= HandleUnitRemoved;
		}

		spawnedUnits.Remove(unit);

		CheckClearCondition();
	}

	private void CheckClearCondition()
	{
		if (!isSpawnFinished)
		{
			return;
		}

		if (spawnedUnits.Count > 0)
		{
			return;
		}

		if (GameManager.Instance == null)
		{
			return;
		}

		if (GameManager.Instance.IsGameEnded)
		{
			return;
		}

		GameManager.Instance.GameClear();
	}

	private void HandleGridChanged()
	{
		if (recalculateCoroutine != null)
		{
			StopCoroutine(recalculateCoroutine);
		}

		recalculateCoroutine = StartCoroutine(RecalculatePathsNextFrame());
	}

	private IEnumerator RecalculatePathsNextFrame()
	{
		yield return null;

		RecalculateSpawnedUnitPaths();

		recalculateCoroutine = null;
	}

	private void RecalculateSpawnedUnitPaths()
	{
		for (int i = spawnedUnits.Count - 1; i >= 0; i--)
		{
			Unit unit = spawnedUnits[i];

			if (unit == null)
			{
				spawnedUnits.RemoveAt(i);
				continue;
			}

			unit.RecalculatePath();
		}
	}

	public void SpawnSingleUnit(UnitType unitType)
	{
		for (int i = 0; i < waveUnits.Length; i++)
		{
			if (waveUnits[i].unitType == unitType)
			{
				SpawnUnit(waveUnits[i]);
				return;
			}
		}

		Debug.LogWarning($"{unitType} 타입 유닛을 찾을 수 없습니다.");
	}
}