using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EatSpawner : MonoBehaviour {
	[System.Serializable]
	public class PoolEat {
		public Transform transform;
		public bool inUse;
		public PoolEat(Transform t) { transform = t; }
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

	public GameObject Prefab1;
	public GameObject Prefab2;
	public int poolSize;

	public PoolEat[] poolEats;
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
		TapController.OnPlayerEat += OnPlayerEat;
	}

	void OnDisable() {
		GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
		TapController.OnPlayerEat -= OnPlayerEat;
	}

	void OnPlayerEat(int index) {
		CheckDisposeObject (poolEats[index]);
		Spawn ();
	}

	void OnGameOverConfirmed() {
		for (int i = 0; i < poolEats.Length; i++) {
			poolEats[i].Dispose();
			poolEats[i].transform.position = Vector3.one * 1000;
		}
		//		Configure();

		Spawn (1);
	}

	void Configure() {
		//spawning pool objects
		poolEats = new PoolEat[poolSize];
		for (int i = 0; i < poolEats.Length; i++) {
			GameObject go = Instantiate(((Random.Range(0, 100) > 50)? Prefab1 : Prefab2)) as GameObject;
			Transform t = go.transform;
			go.GetComponent<EatScript> ().index = i;
			t.SetParent(transform);
			t.position = Vector3.one * 1000;
			poolEats[i] = new PoolEat(t);
		}
	}

	Transform GetPoolEat() {
		//retrieving first available pool object
		for (int i = 0; i < poolEats.Length; i++) {
			if (!poolEats[i].inUse) {
				poolEats[i].Use();
				return poolEats[i].transform;
			}
		}
		return null;
	}

	void Spawn(int count = 1) {
		//moving pool objects into place
		for (int i = 0; i < count; i++) {
			Transform t = GetPoolEat ();
			if (t == null)
				return;
			Vector3 pos = Vector3.zero;
			pos.y = Random.Range (ySpawnRange.minY, ySpawnRange.maxY);
			pos.x = Random.Range (xSpawnRange.minX, xSpawnRange.maxX);
			t.position = pos;
		}
	}

	void CheckDisposeObject(PoolEat poolObject) {
		//place objects off screen
		poolObject.Dispose();
		poolObject.transform.position = Vector3.one * 1000;
	}
}
