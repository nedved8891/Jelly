using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public struct PoolClass{
	public PoolType type;
	public GameObject prefab;
	public int size;

	public void Init(PoolType _type, GameObject _prefab) {
		type = _type;
		prefab = _prefab;
	}
}

public class InitPool : MonoBehaviour {
	public static InitPool Instance;
	public delegate void Delegate();
	public static event Delegate OnInitPoolEnd;
	public delegate void Delegate2(int value);
	public static event Delegate2 OnSpawnBG;

	public Transform parent;

	public List<PoolClass> pools;

	public List<PoolClass> objs = new List<PoolClass>();

	GameObject go;

	void OnDisable(){
		if (ManagerPool.Instance)
			ManagerPool.Instance.Dispose ();
	}

	void Awake(){
		Instance = this;
	}

	void Start(){
		_OnStartGame ();
	}

	/// <summary>
	/// Заповнення пула
	/// </summary>
	void _OnStartGame(){
		foreach (PoolClass pt in pools){
			ManagerPool.Instance.AddPool (pt.type).PopulateWith(pt.prefab, pt.size, pt.size, 100);
		}

		OnSpawnBG (3);
		OnInitPoolEnd ();
	}

	/// <summary>
	/// Спавн в певну позицію
	/// </summary>
	public Transform Spawn(PoolType _type, Vector3 _position){
		go = ManagerPool.Instance.Spawn (_type, pools [GetIntOfType(_type)].prefab, _position, Quaternion.Euler (0, 0, 0), parent);
		PoolClass pc = new PoolClass (); 
		pc.Init (_type, go);
		objs.Add (pc);
		return go.transform;
	}

	/// <summary>
	/// Повертаєм в пул
	/// </summary>
	void _OnDespawn (int value)
	{
		ManagerPool.Instance.Despawn(objs[value].type, objs[value].prefab);
	}

	void _OnDespawn (PoolClass value)
	{
		ManagerPool.Instance.Despawn(value.type, value.prefab);
	}

	int GetIntOfType(PoolType _type){
		return (int)_type;
	}

	void FixedUpdate(){
		foreach (PoolClass pc in objs.ToArray()){
			if (pc.prefab.activeSelf) {
				if (pc.prefab.transform.position.x < GameController.Instance.PointDestroy.position.x) {
					if ((int)pc.type >= 4 && (int)pc.type <= 13) {
						OnSpawnBG (1);
					}

					_OnDespawn (pc);

					objs.Remove (pc);
				}
			}
		}
	}
}
