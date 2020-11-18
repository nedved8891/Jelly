using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BSH_Prototype;
using UnityEngine.Events;
using System;

[System.Serializable]
public enum Delay{
	sec_15 = 15,
	sec_30 = 30,
	sec_45 = 45,
	sec_60 = 60
}

public class AdsProvider : MonoBehaviour {

	[Header("Екземпляр класа")]
	public static AdsProvider Instance;

	[Header("Час між показами реклами")]
	public Delay delay;

	private float currentTime;
		
	public bool isShowingInterstitial = false;

	/// <summary>
	/// Awake метод
	/// </summary>
	private void Awake(){
		Instance = this;
	}

	private void Start()
	{
		//if user consent was set, just initialize the sdk, else request user consent
		if (Advertisements.Instance.UserConsentWasSet ()) {
			Debug.Log ("Користувач дав згоду");
			Advertisements.Instance.Initialize ();
		} else {
			Debug.Log ("Користувач Не дав згоду");
			Advertisements.Instance.SetUserConsent(false);//персоналізована реклама має показуватись чи ні
			Advertisements.Instance.Initialize();
		}

		currentTime = Time.time;

	}

	private bool isDelayed(){
		if (currentTime + (int)delay < Time.time){
			currentTime = Time.time;
			return true;
		}

		return false;
	}


	/// <summary>
	/// Відбувається, коли екран замерзший(прихований)
	/// </summary>
	public void OnScreenObscured ()
	{
		var current_scene = SceneManager.GetActiveScene ( ).name.ToEnum<Scenes> ( );
		AudioController.Release( );
		//ShowRateUs ( current_scene ); //call before other Scene dependent methods - can override target scene
		SceneLoader.Instance.CheckBigBanner ( current_scene );
	}

	/// <summary>
	/// Показуєм банер
	/// </summary>
	public void ShowBannerAd()
	{
		if (Advertisements.Instance.IsBannerAvailable ()) {
			Advertisements.Instance.ShowBanner (BannerPosition.TOP, BannerType.Banner);
		}
	}

	/// <summary>
	/// Сховати банер якщо він на сцені
	/// </summary>
	public void HideBannerAd()
	{
		if (Advertisements.Instance.IsBannerAvailable ()) {
			Advertisements.Instance.HideBanner ();
        }
        else
        {
            Debug.Log("@@@ Малий банер недоступна, просто перейдем на наступну сцену");
        }
	}

	/// <summary>
	/// Показуєм міжсторінкову рекламу
	/// </summary>
	public void ShowInterstitialAd()
	{
		if (Advertisements.Instance.IsInterstitialAvailable () && isDelayed ())
		{
			isShowingInterstitial = true;
			Advertisements.Instance.ShowInterstitial (InterstitialClosed);
		} else {
			Debug.Log ("@@@ Міжсторінкова реклама недоступна, просто перейдем на наступну сцену");
			SceneLoader.Instance.LoadScene ();
		}
	}

	public void ShowRewardedAd(UnityAction<bool, string> CompleteMethod)
	{
		if (Advertisements.Instance.IsRewardVideoAvailable ()) 
		{
			Advertisements.Instance.ShowRewardedVideo (CompleteMethod);
		} else {
			Debug.Log ("@@@ Відео реклама недоступна, просто перейдем на наступну сцену");
			SceneLoader.Instance.LoadScene ();
		}
	}

	//callback called each time an interstitial is closed
	private void InterstitialClosed(string advertiser)
	{
		if (Advertisements.Instance.debug)
		{
			Debug.Log("Interstitial closed from: " + advertiser + " -> Resume Game ");
			GleyMobileAds.ScreenWriter.Write("Interstitial closed from: " + advertiser + " -> Resume Game ");
		}
		
		MetricaController.Instance.InterstitialAdsViewSuccess();
		
		isShowingInterstitial = true;
		
		SceneLoader.Instance.LoadScene ();
	}

	//callback called each time a rewarded video is closed
	//if completed = true, rewarded video was seen untill the end
	private void VideoComplete(bool completed, string advertiser)
	{
		if (Advertisements.Instance.debug) {
			Debug.Log ("Closed rewarded from: " + advertiser + " -> Completed " + completed);
			GleyMobileAds.ScreenWriter.Write ("Closed rewarded from: " + advertiser + " -> Completed " + completed);
		}

		if (completed == true)
		{
			//user watched the entire video,, he deserves a coin
			//coins += 1000;
			//coinText.text = "Coins: " + coins;
		}
		else
		{
			//no reward for you
		}
	}
}
