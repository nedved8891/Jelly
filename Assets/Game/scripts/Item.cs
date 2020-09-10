using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

	public delegate void ClickAction();
	public static event ClickAction action;

	//The tag of the object that can touch this item
	public string hitTargetTag = "Player";

	//A list of functions that run when this item is touched by the target
	public TouchFunction[] touchFunctions;

	//The playerprefs record name that is affected when picking up this item
	public string playerPrefsName;

	//The value change of the playerprefs record when picking up this item
	public float playerPrefsValue;

	//The effect that is created at the location of this object when it is destroyed
	public Transform hitEffect;

	//The sound that plays when this object is touched
	public AudioClip soundHit;
	public string soundSourceTag = "GameController";

	//This function runs when this obstacle touches another object with a trigger collider
	void  OnTriggerEnter2D ( Collider2D other  ){	
		//Check if the object that was touched has the correct tag
		if ( other.tag == hitTargetTag )
		{
			//Go through the list of functions and runs them on the correct targets
			foreach( TouchFunction touchFunction in touchFunctions )
			{
				//Check that we have a target tag and function name before running
				if ( touchFunction.targetTag != string.Empty && touchFunction.functionName != string.Empty )
				{
					//Run the function
					GameObject.FindGameObjectWithTag(touchFunction.targetTag).SendMessage(touchFunction.functionName, touchFunction.functionParameter);
				}
			}

			//If there is a playerprefs record and a value change, change it
			if ( playerPrefsName != string.Empty && playerPrefsValue != 0 )
			{
				//Get the current value of the player prefs record
				float tempValue = PlayerPrefs.GetFloat( playerPrefsName, 0);

				//Update the record with the new value
				PlayerPrefs.SetFloat( playerPrefsName, tempValue + playerPrefsValue);

				action ();
			}

			//If there is a hit effect, create it
			if ( hitEffect )    Instantiate( hitEffect, transform.position, Quaternion.identity);

			//Destroy the item
			Destroy(gameObject);

			//If there is a sound source and a sound assigned, play it
			if ( soundSourceTag != "" && soundHit )    
			{
				//Reset the pitch back to normal
				GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().pitch = 1;

				//Play the sound
				GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(soundHit);
			}
		}
	}
}
