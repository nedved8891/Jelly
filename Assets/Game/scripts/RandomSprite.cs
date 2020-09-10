using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSprite : MonoBehaviour {

	public Sprite[] sprs;

	void Start() 
	{
		if ( sprs.Length > 0 )    
			gameObject.GetComponent<SpriteRenderer>().sprite = sprs[Mathf.FloorToInt(Random.value * sprs.Length)];
	}
}
