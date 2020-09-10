using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using Spine;

public class ColumnController : MonoBehaviour {
	[Header("Скелетон")]
	public SkeletonAnimation skltn;

	BoxCollider2D coll;

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

	void Start()
	{
		thisTransform = transform;

		// Choose a random starting height for the moving column
		startingHeight = Random.Range(-2.5f,2.5f);
	
		if (skltn)
			skltn.AnimationState.Event += HandleEvent;

		coll = GetComponent<BoxCollider2D> ();
	}

	void Update()
	{
		if ( movingColumn )
		{
			// Move the column
			thisTransform.position = new Vector3( thisTransform.position.x, moveRange.x + (moveRange.y - moveRange.x)/2 + Mathf.Sin(moveSpeed * Time.time + startingHeight) * ((moveRange.y - moveRange.x)/2), thisTransform.position.z);
		}
	}

	//Detect when the player lands on this column
	void  OnCollisionEnter2D(Collision2D coll)
	{
		//If the player lands on top of this column, and the column hasn't been landed on yet, you can land on it
		if ( coll.gameObject.tag == "Player" && coll.transform.position.y > thisTransform.position.y )    
		{
			//The player has landed
			coll.gameObject.SendMessage("PlayerLanded");

			//Give a bonus to the player
			if ( giveBonus == true )    coll.gameObject.SendMessage("ChangeScore", thisTransform);

			if (isDestroy)
				Destroy ();

			giveBonus = false;

			// Attach the player to this column
			coll.transform.parent = thisTransform;
		}
	}

	//Don't give bonus when landing on this column
	void  NoBonus()
	{
		giveBonus = false;
	}

	void  IsDestroy()
	{
		isDestroy = true;
	}

	void Destroy(float _delay = 2){
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
