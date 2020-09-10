using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipesScript : MonoBehaviour {

	public List<Alignment> pipes;

	public float offsetX;

	public List<Sprite> sprites;

	PolygonCollider2D coll;

	public void Awake(){
		pipes = new List<Alignment>( GetComponentsInChildren<Alignment>()); 
	}
		
	public void Show(){
		for (int i = 0; i < pipes.Count; i++){
			pipes [i].Show (.1f, 0);
			pipes [i].gameObject.GetComponent<SpriteRenderer> ().sprite = sprites [Random.Range (0, sprites.Count)];
		} 
	}

	public void Hide(){
		for (int i = 0; i < pipes.Count; i++){
			pipes [i].Hide (0.1f, offsetX);
		} 
	}

}
