using UnityEngine;
using System.Collections;

public class GlobalMusic:MonoBehaviour 
{
	//Тег музичного джерела
	public string musicTag = "Music";
	
	//Час, коли цей екземпляр джерела музики був у грі
	internal float instanceTime = 0;

	void  Awake()
	{
		//Знайдіть всі об'єкти музики на сцені
		GameObject[] musicObjects = GameObject.FindGameObjectsWithTag(musicTag);
		
		//Зберігайте лише музичний об'єкт, який був у грі більше, ніж за 0 секунд
		if ( musicObjects.Length > 1 )
		{
			foreach( var musicObject in musicObjects )
			{
				if ( musicObject.GetComponent<GlobalMusic>().instanceTime <= 0 )    Destroy(gameObject);
			}
		}
	}

	void  Start()
	{
		//Не руйнуйте цей об'єкт під час завантаження нової сцени
		DontDestroyOnLoad(transform.gameObject);
	}
	
}
