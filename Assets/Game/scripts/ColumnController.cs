using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using Spine;
using Random = UnityEngine.Random;

public class ColumnController : MonoBehaviour {
	[Header("Скелетон")]
	public SkeletonAnimation skltn;

	[HideInInspector]
	public BoxCollider2D coll;

	internal Transform thisTransform;

	//Should this column give a bonus when landed on?
	public bool giveBonus = true;

	//Should this column give a bonus when landed on?
	public bool isDestroy = false;

	// Make this column move within a limited range
	public bool movingColumn = false;

	// The starting position of a moving column
	internal float startingHeight;

	// The range in which the column moves
	internal Vector2 moveRange;

	// The vertical movement speed of the column
	public float moveSpeed = 1;

	private void Start()
	{
		thisTransform = transform;

		// Choose a random starting height for the moving column
		startingHeight = Random.Range(-2.5f,2.5f);
	
		if (skltn)
			skltn.AnimationState.Event += HandleEvent;

		coll = GetComponent<BoxCollider2D> ();
	}

	private void Update()
	{
		if ( movingColumn )
		{
			// Move the column
			thisTransform.position = new Vector3( thisTransform.position.x, moveRange.x + (moveRange.y - moveRange.x)/2 + Mathf.Sin(moveSpeed * Time.time + startingHeight) * ((moveRange.y - moveRange.x)/2), thisTransform.position.z);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ( other.gameObject.CompareTag("Player") && other.transform.position.y > thisTransform.position.y )    
		{
			other.gameObject.SendMessage("PlayerLanded");

			if ( giveBonus == true )    other.gameObject.SendMessage("ChangeScore", thisTransform);
			
			if (isDestroy)
				Destroy ();

			giveBonus = false;

			other.transform.parent = thisTransform;
		}
	}

	//Don't give bonus when landing on this column
	private void  NoBonus()
	{
		giveBonus = false;
	}

	private void  IsDestroy(bool value)
	{
		isDestroy = value;
		
		if(skltn)
			skltn.state.SetAnimation (0, "Idle", true);
	}

	private void Destroy(float _delay = 2){
		DOVirtual.DelayedCall (_delay, ()=>{
			if(skltn)
				skltn.state.SetAnimation (0, "Destroy", false);
		});
	}

	void HandleEvent (TrackEntry trackEntry, Spine.Event e) {
		// Play some sound if the event named "footstep" fired.
		if (e.Data.Name == "Destroy") {
			coll.enabled = false;
		}
	}

	/// <summary>
	/// Sets the vertical move range of the column
	/// </summary>
	/// <param name="newMoveRange">The new move range for this column</param>
	void SetMoveRange( Vector2 newMoveRange )
	{
		moveRange = newMoveRange;
	}

	void OnBecameInvisible(){
		Debug.Log("OnBecameVisible");
	}
}
