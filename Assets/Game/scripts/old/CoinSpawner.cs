using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CoinSpawner : MonoBehaviour {
	[System.Serializable]
	public class PoolCoin {
		public Transform transform;
		public bool inUse;
		public PoolCoin(Transform t) { transform = t; }
		public void Use() { inUse = true; }
		public void Dispose() { inUse = false; }
	}

	[System.Serializable]
	public struct YSpawnRange {
		public float minY;
		public float maxY;
	}

	[System.Serializable]
	public struct XSpawnRange {
		public float minX;
		public float maxX;
	}

	public YSpawnRange ySpawnRange;
	public XSpawnRange xSpawnRange;

	public GameObject Prefab;
	public int poolSize;

	public PoolCoin[] poolCoins;
	GameManager game;

	void Awake() {
		Configure();
	}

	void Start() {
		game = GameManager.Instance;

		Spawn (1);
	}

	void OnEnable() {
		GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
		TapController.OnPlayerCoin += OnPlayerCoin;
	}

	void OnDisable() {
		GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
		TapController.OnPlayerCoin -= OnPlayerCoin;
	}

	void OnPlayerCoin(int index) {
		CheckDisposeObject (poolCoins[index]);
		Spawn ();
	}

	void OnGameOverConfirmed() {
		for (int i = 0; i < poolCoins.Length; i++) {
			poolCoins[i].Dispose();
			poolCoins[i].transform.position = Vector3.one * 1000;
		}
//		Configure();

		Spawn (1);
	}

	void Configure() {
		//spawning pool objects
		poolCoins = new PoolCoin[poolSize];
		for (int i = 0; i < poolCoins.Length; i++) {
			GameObject go = Instantiate(Prefab) as GameObject;
			Transform t = go.transform;
			go.GetComponent<CoinScript> ().index = i;
			t.SetParent(transform);
			t.position = Vector3.one * 1000;
			poolCoins[i] = new PoolCoin(t);
		}
	}

	Transform GetPoolCoin() {
		//retrieving first available pool object
		for (int i = 0; i < poolCoins.Length; i++) {
			if (!poolCoins[i].inUse) {
				poolCoins[i].Use();
				return poolCoins[i].transform;
			}
		}
		return null;
	}

	void Spawn(int count = 1) {
		//moving pool objects into place
		for (int i = 0; i < count; i++) {
			Transform t = GetPoolCoin ();
			if (t == null)
				return;
			Vector3 pos = Vector3.zero;
			pos.y = Random.Range (ySpawnRange.minY, ySpawnRange.maxY);
			pos.x = Random.Range (xSpawnRange.minX, xSpawnRange.maxX);
			t.position = pos;
		}
	}

	void CheckDisposeObject(PoolCoin poolObject) {
		//place objects off screen
		poolObject.Dispose();
		poolObject.transform.position = Vector3.one * 1000;
	}
}
