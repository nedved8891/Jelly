using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public struct Hat{
	public Sprite colorSprire;
	public Sprite shadowSprite;
	public int costSweet;
}

public class HatsScript : MonoBehaviour {
	
	public delegate void ClickAction();
	public static event ClickAction SwitchHat;

	public List<Hat> hats;

	public Image color;

	public Image shadow;

	void Start (){
		SetIcons ();

		Item.action += Item_action;
	}

	void Item_action ()
	{
		SetProgress ();
	}

	void SetIcons(){
		color.sprite = hats [PlayerPrefs.GetInt ("Hat", 0)].colorSprire;
		color.fillAmount = 0;

		shadow.sprite = hats [PlayerPrefs.GetInt ("Hat", 0)].shadowSprite;
	}

	void SetProgress(){
		color.DOFillAmount (color.fillAmount + 1.0f/hats [PlayerPrefs.GetInt("Hat", 0)].costSweet, 1)
			.OnComplete(()=>{
				if (color.fillAmount >= 1){
					PlayerPrefs.SetInt ("Hat", PlayerPrefs.GetInt("Hat", 0) + 1);
					SwitchHat ();

					if (PlayerPrefs.GetInt("Hat", 0) >= 5)
						PlayerPrefs.SetInt ("Hat", 0);

					SetIcons();
				}
			});
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.P)){
			SetProgress ();
		}
	}
}
