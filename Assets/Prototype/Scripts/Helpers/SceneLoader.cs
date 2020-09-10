using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

namespace BSH_Prototype
{
	
	public class SceneLoader : MonoBehaviour {

		[Header("Екземпляр класа")]
		public static SceneLoader 
			Instance;

		[Header("Сцени, які варто вказати в редакторі")]
		public Scenes 
			first_scene = Scenes.Level_4,
			logo_scene = Scenes.Logo;

		private CanvasGroup
			fade_group;     

		private Scenes 
			_target_scene;

		void OnEnable()
		{
			SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		void OnDisable()
		{
			SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
		}

		/// <summary>
		/// Awake Метод
		/// </summary>
		void Awake ()
		{
			if (!Instance)
			{
				Instance = this;
				DontDestroyOnLoad ( gameObject );
			}
		}


		/// <summary>
		/// Метод переходу на іншу сцену
		/// </summary>
		public void SwitchToScene (Scenes target_scene)
		{
			print ("Хочемо перейти на сцену: <<" + target_scene+">>");
			SetTargetScene ( target_scene );

			UIFader.Instance.FadeIn ();
		}

		/// <summary>
		/// Загальний спосіб завантаження сцени
		/// </summary>
		public void LoadScene ()
		{
			UIFader.Instance.FadeOut ();

			SceneManager.LoadScene( ""+GetTargetScene ( ).ToString ( ));

		}

		/// <summary>
		///	Метод, викликається, коли загрузилась сцена
		/// </summary>
		/// <param name="_scene">Scene.</param>
		/// <param name="_sceneMode">Scene mode.</param>
		void SceneManager_sceneLoaded (Scene _scene, LoadSceneMode _sceneMode)
		{
			print ("Загрузилась сцена <<" + _scene.name+ ">>");

			CheckSmallBanner ((Scenes)Enum.Parse(typeof(Scenes), _scene.name));
		}

		/// <summary>
		/// Показати міжсторінковий банер
		/// </summary>
		/// <param name="current_scene">Current scene.</param>
		public void CheckBigBanner (Scenes current_scene)
		{
			if (!SceneLoaderSettings.not_allowed_interstitial.Contains ( current_scene ))
			{
				print ("Після сцени <<" + current_scene + ">> потрібно показати міжсторінковий банер");

				if (Utils.IsMobilePlatform ()) 
				{
//					AdsScript.Instance.ShowInterstitialAd ();
					AdsProvider.Instance.ShowInterstitialAd ();
				} else	{
					print ("Це не моюбільна платформа");
					LoadScene ();
				}
			}
			else
			{
				print ("Після сцени <<" + current_scene + ">> не потрібно показувати міжсторінковий банер");
				LoadScene ();
			}
		}

		/// <summary>
		/// Показати малий банер
		/// </summary>
		public void CheckSmallBanner (Scenes current_scene)
		{
			if (!SceneLoaderSettings.not_allowed_small_banner.Contains (current_scene)) {
				print ("На сцені <<" + current_scene + ">> потрібно показати малий банер");
				if (Utils.IsMobilePlatform ()) {
//					AdsScript.Instance.ShowBannerAd ();
					AdsProvider.Instance.ShowBannerAd ();
				} else {
					print ("Це не моюбільна платформа");
				}
			} else {
//				AdsScript.Instance.HideBannerAd ();
				AdsProvider.Instance.HideBannerAd ();
				print ("На сцені <<" + current_scene + ">> не потрібно показувати малий банер");
			}
		}
			
		/// <summary>
		/// Встановити таргет на сцену
		/// </summary>
		/// <param name="scene">Scene.</param>
		public void SetTargetScene (Scenes scene)
		{
			_target_scene = scene;
		}

		/// <summary>
		/// отримати таргет на сцену
		/// </summary>
		/// <returns>The target scene.</returns>
		public Scenes GetTargetScene ()
		{
			return _target_scene;
		}

		/// <summary>
		/// Отримати поточну сцену
		/// </summary>
		/// <returns>The current scene.</returns>
		public Scenes GetCurrentScene ()
		{
			return GetStringCurrentScene ( ).ToEnum<Scenes> ( );
		}

		/// <summary>
		/// Отримати поточну сцену в стрінг форматі
		/// </summary>
		/// <returns>The string current scene.</returns>
		public string GetStringCurrentScene ()
		{
			return SceneManager.GetActiveScene ( ).name;
		}
	}

}
