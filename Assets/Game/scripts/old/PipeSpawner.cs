using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolObject {
	public Transform transform;
	public bool inUse;
	public PoolObject(Transform t) { transform = t; }
	public void Use() { inUse = true; }
	public void Dispose() { inUse = false; }
}

[System.Serializable]
public struct Pipe{
	[Header("Зміщення по Y")]
	public float offsetY;
}

[System.Serializable]
public struct Pipes{
	[Header("Список колючок")]
	public List<Pipe> pipes;
}

public class PipeSpawner : MonoBehaviour {
	[Header("Зміщення по X за межі екрана")]
	public float offsetX = 2;

	public GameManager.Directions isLeft;

	[Header("Список із списків шипів")]
	public List<Pipes> listPipe;

	public List<PoolObject> poolObjectsType1;
	public List<PoolObject> poolObjectsType2;
	public List<PoolObject> poolObjectsType3;

	void OnEnable() {
//		TapController.OnPlayerScored += Configure;
		GameManager.OnGameOverConfirmed += Hide;
		GameManager.OnGameChangeDirection += Configure;
	}

	void OnDisable() {
//		TapController.OnPlayerScored -= Configure;
		GameManager.OnGameOverConfirmed -= Hide;
		GameManager.OnGameChangeDirection -= Configure;
	}

	void Awake(){
		if (isLeft == GameManager.Directions.Left) {
			offsetX = -1 * offsetX;
		} else {
			offsetX = offsetX;
		}
	}

	[Header("Список поточних шипів")]
	public List<Alignment> listCurrentPipes;

	public void Configure() {
		if (isLeft == GameManager.Instance.direction) {
			Transform t;
			int rnd = Random.Range (0, listPipe.Count - 1);
			int count = listPipe [rnd].pipes.Count - 1; //скільки треба шипів зробити

			listCurrentPipes.Clear ();

			for (int i = 0; i <= count; i++) {
				int type = Random.Range (1, 3);

				if (type == 1) {
					t = GetPoolObject (poolObjectsType1);
				} else if (type == 2) {
					t = GetPoolObject (poolObjectsType2);
				} else {
					t = GetPoolObject (poolObjectsType3);
				}

				Alignment al = t.gameObject.GetComponent<Alignment> ();
				listCurrentPipes.Add (al);

				al.SetOffset (offsetX, listPipe [rnd].pipes [i].offsetY);

				al.Show ();
			}
		} else {
			Hide ();
		}
	}

	public void Hide(){
		foreach (Alignment al in listCurrentPipes) {
			al.Hide (1, offsetX);
		}

		listCurrentPipes.Clear ();

		foreach (PoolObject po in poolObjectsType1) {
			po.Dispose ();
		}
		foreach (PoolObject po in poolObjectsType2) {
			po.Dispose ();
		}
		foreach (PoolObject po in poolObjectsType3) {
			po.Dispose ();
		}
	}

	Transform GetPoolObject(List<PoolObject> poolObjects) {
		for (int i = 0; i < poolObjects.Count; i++) {
			if (!poolObjects [i].inUse) {
				poolObjects [i].Use ();
				return poolObjects [i].transform;
			}
		}
		return null;
	}
}
