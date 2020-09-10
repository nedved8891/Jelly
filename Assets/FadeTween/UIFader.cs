using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BSH_Prototype;

public class UIFader : MonoBehaviour {

	public static UIFader Instance;

    public CanvasGroup uiElement;

	public float lerpTime = .5f;

	void Awake(){
		Instance = this;
	}

//	void Start ()
//	{
//		SceneLoader.LoadSceneDelegate += FadeOut;
//	}
		
    public void FadeIn()
    {
		StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 1, lerpTime));
    }

    public void FadeOut()
    {
		StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 0, lerpTime));
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float lerpTime = 1)
	{

		float _timeStartedLerping = Time.time;
		float timeSinceStarted = Time.time - _timeStartedLerping;
		float percentageComplete = timeSinceStarted / lerpTime;

		while (true)
		{
			timeSinceStarted = Time.time - _timeStartedLerping;
			percentageComplete = timeSinceStarted / lerpTime;

			float currentValue = Mathf.Lerp(start, end, percentageComplete);

            cg.alpha = currentValue;

            if (percentageComplete >= 1) break;
			yield return new WaitForFixedUpdate();
		}

		CompleteFade (end);
	}

	public void CompleteFade(float _end){
		print("Complete Fade : " + Time.time);
		if (_end == 1){
//			AdsScript.Instance.OnScreenObscured ();
			AdsProvider.Instance.OnScreenObscured ();
		}else{

		}
	}
}
