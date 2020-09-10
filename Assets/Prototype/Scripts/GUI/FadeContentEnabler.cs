using UnityEngine;
using BSH_Prototype;

public class FadeContentEnabler :MonoBehaviour
{

	private GameObject [] children;

	void Awake ()
	{
		children = new GameObject [transform.childCount];
		for (int i = 0; i < children.Length; i++)
		{
			children [i] = transform.GetChild ( i ).gameObject;
		}
	}

	void OnEnable ()
	{
		EnableChildren ( false );
//		SceneLoader.OnLoadProgressChange += OnLoadProgressChange;
	}

	void OnDisable ()
	{
//		SceneLoader.OnLoadProgressChange -= OnLoadProgressChange;
	}

	void OnLoadProgressChange (float progress)
	{
		if (progress > 0)
		{
			EnableChildren ( true );
		}
	}

	void EnableChildren (bool param)
	{
		for (int i = 0; i < children.Length; i++)
		{
			children [i].SetActive ( param );
		}
	}
}
