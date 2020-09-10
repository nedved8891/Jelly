using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BSH_Prototype;

public class AdsScript : MonoBehaviour {

	[Header("Екземпляр класа")]
	public static AdsScript Instance;

	private const string interstitial = "ca-app-pub-8205304172271719/3513137476";
	private const string banner = "ca-app-pub-8205304172271719/9880787664";

	private InterstitialAd interAd;

	private float 
		wait_time_load_interstitial = 2.0f;

	private bool
		interstitialShow = false;

	private Coroutine waitCorotine;

	private BannerView bannerAd;

	private AdRequest request;

	/// <summary>
	/// Awake метод
	/// </summary>
	void Awake(){
		Instance = this;
	}

	/// <summary>
	/// Створюєм запит і запускаєм білд
	/// </summary>
	public void CreateRequest(){
		//***Тестування на девайсі***
		request = new AdRequest.Builder()
			.AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
			.AddTestDevice("32806DC1BF19890E")  				// My test device.
			.Build();

		//***Для виробництва, коли подає додаток***
		//request = new AdRequest.Builder().Build();
	}

	/// <summary>
	/// Відбувається, коли екран замерзший(прихований)
	/// </summary>
	public void OnScreenObscured ()
	{
		Scenes current_scene = SceneManager.GetActiveScene ( ).name.ToEnum<Scenes> ( );
		AudioController.Release( );
		//ShowRateUs ( current_scene ); //call before other Scene dependent methods - can override target scene
		SceneLoader.Instance.CheckBigBanner ( current_scene );
	}

	#region Малий банер
	/// <summary>
	/// Показуєм банер
	/// </summary>
	public void ShowBannerAd()
	{

		CreateRequest ();

		bannerAd = new BannerView(banner, AdSize.Banner, AdPosition.Bottom);
		bannerAd.LoadAd(request);

		bannerAd.OnAdLoaded += BanerAd_OnAdLoaded;
	}

	/// <summary>
	/// Сховати банер якщо він на сцені
	/// </summary>
	public void HideBannerAd()
	{
		if (bannerAd != null) {
			bannerAd.Hide ();
			bannerAd.Destroy ();
		}
	}

	/// <summary>
	/// Реєструєм метод, що включається, коли банер загрузиться
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="args">Arguments.</param>
	void BanerAd_OnAdLoaded (object sender, System.EventArgs args)
	{
		print ("Загрузився малий банер");
	}
	#endregion
		
	#region Міжсторінковий банер
	/// <summary>
	/// Показуєм міжсторінкову рекламу
	/// </summary>
	public void ShowInterstitialAd()
	{
		CreateRequest ();

		interAd = new InterstitialAd (interstitial);

		if (interAd.IsLoaded()) {
			print ("Міжсторінкова реклама вже була загружена в пам*яті");
			interAd.Show ();
		} else {
			interstitialShow = true;

			interAd.LoadAd (request);

			interAd.OnAdLoaded += InterAd_OnAdLoaded;

			interAd.OnAdClosed += InterAd_OnAdClosed;

			waitCorotine = StartCoroutine (WaitLoad (wait_time_load_interstitial));
		}
	}

	/// <summary>
	/// Реєструєм метод, що включиться, коли інтер загрузиться
	/// </summary>
	void InterAd_OnAdLoaded (object sender, System.EventArgs args)
	{
		if (interstitialShow) {
			print ("Загрузилася міжсторінкова реклама");
			interAd.Show ();
			StopCoroutine (waitCorotine);
		} else {
			print ("Пізно загрузитись");
		}
	}

	/// <summary>
	/// Реєструєм метод, що включається, коли інтер закриється
	/// </summary>
	void InterAd_OnAdClosed (object sender, System.EventArgs args)
	{
		print ("SceneLoader: Current scene =" + SceneLoader.Instance.GetStringCurrentScene());
		print ("PlayerPrefs: SceneToLoad =" + PlayerPrefs.GetString("SceneToLoad"));

		SceneLoader.Instance.LoadScene ();

		//ShowSmallBanner (PlayerPrefs.GetString("SceneToLoad").ToEnum<Scenes> ( ));
	}

	/// <summary>
	/// Реєструєм метод, що включається, коли інтер загрузився попередньо
	/// </summary>
	void InterAd_OnAdPreviousDownload (object sender, System.EventArgs args)
	{
		print ("Попередня загрузка завершилась");
	}

	/// <summary>
	/// Чекаєм деякий час чи загрузиться чи ні реклама
	/// </summary>
	/// <returns>The load.</returns>
	/// <param name="time">Time.</param>
	IEnumerator WaitLoad (float time)
	{
		print ("Чекаєм " + time + "секунд ... " + Time.time);
		yield return new WaitForSeconds (time);
		print ("Час звкінчився ... " + Time.time);
		if (!interAd.IsLoaded ()) {
			print ("Не успів загрузитись");

			SceneLoader.Instance.LoadScene ();

			interstitialShow = false;
		} else {
			print ("Загрузився в останній момент");
		}
	}

	/// <summary>
	/// Попередня загрузка реклами
	/// </summary>
	public void PreviousDownload(){
		if (interAd != null) {
			if (interAd.IsLoaded ()) {
				print ("Попередня загрузка не потрібна вже загрузили попередньо");
				return;
			}
		}
		
		print (" Попередня загрузка ... ");
		CreateRequest ();

		interAd = new InterstitialAd (interstitial);
		interAd.LoadAd (request);

		interAd.OnAdLoaded += InterAd_OnAdPreviousDownload;

		//waitCorotine = StartCoroutine (WaitLoad (wait_time_load_interstitial)); //це походу тут не треба сбробую закоментувати, адже навіщо перевіряти чи пройшов час в попередній загрузці чи ні
	}
	#endregion
}
