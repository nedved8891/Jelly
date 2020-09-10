namespace GleyMobileAds
{
    using UnityEngine.Events;
    using UnityEngine;
#if USE_ADMOB
    using GoogleMobileAds.Api;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections;
#endif

    public class CustomAdmob : MonoBehaviour, ICustomAds
    {
#if USE_ADMOB
        private UnityAction<bool> OnCompleteMethod;
        private UnityAction<bool, string> OnCompleteMethodWithAdvertiser;
        private UnityAction OnInterstitialClosed;
        private UnityAction<string> OnInterstitialClosedWithAdvertiser;
        private UnityAction<bool, BannerPosition, BannerType> DisplayResult;
        private InterstitialAd interstitial;
        private BannerView banner;
        private RewardBasedVideoAd rewardedVideo;
        private BannerPosition position;
        private BannerType bannerType;
        private string rewardedVideoId;
        private string interstitialId;
        private string bannerId;
        private string consent;
        private string designedForFamilies;
        private bool directedForChildren;
        private readonly int maxRetryCount = 10;
        private int currentRetryRewardedVideo;
        private bool debug;
        private bool initialized;
        private bool triggerCompleteMethod;
        private bool bannerLoaded;
        private bool bannerUsed;
        private bool rewardedVideoAdClosed;
        private bool interstitialAdCLosed;


        /// <summary>
        /// Initializing Admob
        /// </summary>
        /// <param name="consent">user consent -> if true show personalized ads</param>
        /// <param name="platformSettings">contains all required settings for this publisher</param>
        public void InitializeAds(GDPRConsent consent, List<PlatformSettings> platformSettings)
        {
            debug = Advertisements.Instance.debug;
            if (initialized == false)
            {
                if (debug)
                {
                    Debug.Log(this + " " + "Start Initialization");
                    ScreenWriter.Write(this + " " + "Start Initialization");
                }

                //get settings
#if UNITY_ANDROID
                PlatformSettings settings = platformSettings.First(cond => cond.platform == SupportedPlatforms.Android);
#endif
#if UNITY_IOS
                PlatformSettings settings = platformSettings.First(cond => cond.platform == SupportedPlatforms.iOS);
#endif
                //apply settings
                interstitialId = settings.idInterstitial.id;
                bannerId = settings.idBanner.id;
                rewardedVideoId = settings.idRewarded.id;

                if (settings.directedForChildren == true)
                {
                    designedForFamilies = "true";
                }
                else
                {
                    designedForFamilies = "false";
                }
                directedForChildren = settings.directedForChildren;

                MobileAds.SetiOSAppPauseOnBackground(true);
                MobileAds.Initialize(settings.appId.id);
                //verify settings
                if (debug)
                {
                    Debug.Log(this + " Banner ID: " + bannerId);
                    ScreenWriter.Write(this + " Banner ID: " + bannerId);
                    Debug.Log(this + " Interstitial ID: " + interstitialId);
                    ScreenWriter.Write(this + " Interstitial ID: " + interstitialId);
                    Debug.Log(this + " Rewarded Video ID: " + rewardedVideoId);
                    ScreenWriter.Write(this + " Rewarded Video ID: " + rewardedVideoId);
                    Debug.Log(this + " Directed for children: " + directedForChildren);
                    ScreenWriter.Write(this + " Directed for children: " + directedForChildren);
                }

                //preparing Admob SDK for initialization
                if (consent == GDPRConsent.Unset || consent == GDPRConsent.Accept)
                {
                    this.consent = "0";
                }
                else
                {
                    this.consent = "1";
                }

                //add rewarded video listeners
                rewardedVideo = RewardBasedVideoAd.Instance;
                rewardedVideo.OnAdLoaded += RewardedVideoLoaded;
                rewardedVideo.OnAdRewarded += RewardedVideoWatched;
                rewardedVideo.OnAdFailedToLoad += RewardedVideoFailed;
                rewardedVideo.OnAdClosed += OnAdClosed;

                //start loading ads
                LoadRewardedVideo();
                LoadInterstitial();
                initialized = true;
            }
        }


        /// <summary>
        /// Updates consent at runtime
        /// </summary>
        /// <param name="consent">the new consent</param>
        public void UpdateConsent(GDPRConsent consent)
        {
            if (consent == GDPRConsent.Unset || consent == GDPRConsent.Accept)
            {
                this.consent = "0";
            }
            else
            {
                this.consent = "1";
            }

            Debug.Log(this + " Update consent to " + consent);
            ScreenWriter.Write(this + " Update consent to " + consent);
        }

        /// <summary>
        /// Check if Admob interstitial is available
        /// </summary>
        /// <returns>true if an interstitial is available</returns>
        public bool IsInterstitialAvailable()
        {
            if (interstitial != null)
            {
                return interstitial.IsLoaded();
            }
            return false;
        }


        /// <summary>
        /// Show Admob interstitial
        /// </summary>
        /// <param name="InterstitialClosed">callback called when user closes interstitial</param>
        public void ShowInterstitial(UnityAction InterstitialClosed)
        {
            if (interstitial.IsLoaded())
            {
                OnInterstitialClosed = InterstitialClosed;
                interstitial.Show();
            }
        }


        /// <summary>
        /// Show Admob interstitial
        /// </summary>
        /// <param name="InterstitialClosed">callback called when user closes interstitial</param>
        public void ShowInterstitial(UnityAction<string> InterstitialClosed)
        {
            if (interstitial.IsLoaded())
            {
                OnInterstitialClosedWithAdvertiser = InterstitialClosed;
                interstitial.Show();
            }
        }

        /// <summary>
        /// Check if Admob rewarded video is available
        /// </summary>
        /// <returns>true if a rewarded video is available</returns>
        public bool IsRewardVideoAvailable()
        {
            if (rewardedVideo != null)
            {
                return rewardedVideo.IsLoaded();
            }
            return false;
        }


        /// <summary>
        /// Show Admob rewarded video
        /// </summary>
        /// <param name="CompleteMethod">callback called when user closes the rewarded video -> if true video was not skipped</param>
        public void ShowRewardVideo(UnityAction<bool> CompleteMethod)
        {
            if (IsRewardVideoAvailable())
            {
                OnCompleteMethod = CompleteMethod;
                triggerCompleteMethod = true;
                rewardedVideo.Show();
            }
        }


        /// <summary>
        /// Show Admob rewarded video
        /// </summary>
        /// <param name="CompleteMethod">callback called when user closes the rewarded video -> if true video was not skipped</param>
        public void ShowRewardVideo(UnityAction<bool, string> CompleteMethod)
        {
            if (IsRewardVideoAvailable())
            {
                OnCompleteMethodWithAdvertiser = CompleteMethod;
                triggerCompleteMethod = true;
                rewardedVideoAdClosed = false;
                rewardedVideo.Show();
            }
        }


        /// <summary>
        /// Check if Admob banner is available
        /// </summary>
        /// <returns>true if a banner is available</returns>
        public bool IsBannerAvailable()
        {
            return true;
        }


        /// <summary>
        /// Show Admob banner
        /// </summary>
        /// <param name="position"> can be TOP or BOTTOM</param>
        ///  /// <param name="bannerType"> can be Banner or SmartBanner</param>
        public void ShowBanner(BannerPosition position, BannerType bannerType, UnityAction<bool, BannerPosition, BannerType> DisplayResult)
        {
            bannerLoaded = false;
            bannerUsed = true;
            this.DisplayResult = DisplayResult;
            if (banner != null)
            {
                if (this.position == position && this.bannerType == bannerType)
                {
                    if (debug)
                    {
                        Debug.Log(this + " " + "Show banner");
                        ScreenWriter.Write(this + " " + "Show Banner");
                    }
                    bannerLoaded = true;
                    banner.Show();
                    if (this.DisplayResult != null)
                    {
                        this.DisplayResult(true, position, bannerType);
                        this.DisplayResult = null;
                    }
                }
                else
                {
                    LoadBanner(position, bannerType);
                }
            }
            else
            {
                LoadBanner(position, bannerType);
            }
        }


        /// <summary>
        /// Used for mediation purpose
        /// </summary>
        public void ResetBannerUsage()
        {
            bannerUsed = false;
        }


        /// <summary>
        /// Used for mediation purpose
        /// </summary>
        /// <returns>true if current banner failed to load</returns>
        public bool BannerAlreadyUsed()
        {
            return bannerUsed;
        }



        /// <summary>
        /// Hides Admob banner
        /// </summary>
        public void HideBanner()
        {
            if (debug)
            {
                Debug.Log(this + " " + "Hide banner");
                ScreenWriter.Write(this + " " + "Hide banner");
            }
            if (banner != null)
            {
                if (bannerLoaded == false)
                {
                    //if banner is not yet loaded -> destroy so it cannot load later in the game
                    banner.Destroy();
                }
                else
                {
                    //hide the banner -> will be available later without loading
                    banner.Hide();
                }
            }
        }


        /// <summary>
        /// Loads an Admob banner
        /// </summary>
        /// <param name="position">display position</param>
        /// <param name="bannerType">can be normal banner or smart banner</param>
        private void LoadBanner(BannerPosition position, BannerType bannerType)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Start Loading Banner");
                ScreenWriter.Write(this + " " + "Start Loading Banner");
            }

            //setup banner
            if (banner != null)
            {
                banner.Destroy();
            }

            this.position = position;
            this.bannerType = bannerType;

            switch (position)
            {
                case BannerPosition.BOTTOM:
                    if (bannerType == BannerType.SmartBanner)
                    {
                        banner = new BannerView(bannerId, AdSize.SmartBanner, AdPosition.Bottom);
                    }
                    else
                    {
                        banner = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
                    }
                    break;
                case BannerPosition.TOP:
                    if (bannerType == BannerType.SmartBanner)
                    {
                        banner = new BannerView(bannerId, AdSize.SmartBanner, AdPosition.Top);
                    }
                    else
                    {
                        banner = new BannerView(bannerId, AdSize.Banner, AdPosition.Top);
                    }
                    break;
            }

            //add listeners
            banner.OnAdLoaded += BannerLoadSucces;
            banner.OnAdFailedToLoad += BannerLoadFailed;

            //request banner
            AdRequest request = new AdRequest.Builder().AddExtra("npa", consent).AddExtra("is_designed_for_families", designedForFamilies).TagForChildDirectedTreatment(directedForChildren).Build();
            banner.LoadAd(request);
        }


        /// <summary>
        /// Admob specific event triggered after banner was loaded 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BannerLoadSucces(object sender, EventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Banner Loaded");
                ScreenWriter.Write(this + " " + "Banner Loaded");
            }
            bannerLoaded = true;
            if (DisplayResult != null)
            {
                DisplayResult(true, position, bannerType);
                DisplayResult = null;
            }
        }


        /// <summary>
        /// Admob specific event triggered after banner failed to load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BannerLoadFailed(object sender, AdFailedToLoadEventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Banner Failed To Load " + e.Message);
                ScreenWriter.Write(this + " " + "Banner Failed To Load " + e.Message);
            }
            banner = null;
            bannerLoaded = false;
            if (DisplayResult != null)
            {
                DisplayResult(false, position, bannerType);
                DisplayResult = null;
            }
        }


        /// <summary>
        /// Loads an Admob interstitial
        /// </summary>
        private void LoadInterstitial()
        {
            if (debug)
            {
                Debug.Log(this + " " + "Start Loading Interstitial");
                ScreenWriter.Write(this + " " + "Start Loading Interstitial");
            }

            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            //setup and load interstitial
            interstitial = new InterstitialAd(interstitialId);

            interstitial.OnAdLoaded += InterstitialLoaded;
            interstitial.OnAdClosed += InterstitialClosed;
            interstitial.OnAdFailedToLoad += InterstitialFailed;


            AdRequest request = new AdRequest.Builder().AddExtra("npa", consent).AddExtra("is_designed_for_families", designedForFamilies).TagForChildDirectedTreatment(directedForChildren).Build();
            interstitial.LoadAd(request);
        }


        /// <summary>
        /// Admob specific event triggered after an interstitial was loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterstitialLoaded(object sender, EventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Interstitial Loaded");
                ScreenWriter.Write(this + " " + "Interstitial Loaded");
            }
        }


        /// <summary>
        /// Admob specific event triggered after an interstitial was closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterstitialClosed(object sender, EventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Reload Interstitial");
                ScreenWriter.Write(this + " " + "Reload Interstitial");
            }

            //reload interstitial
            LoadInterstitial();

            //trigger complete event
            interstitialAdCLosed = true;
#if !UNITY_ANDROID
            StartCoroutine(CompleteMethodInterstitial());
#endif

        }

        /// <summary>
        ///  Because Admob has some problems when used in multithreading aplications with Unity a frame needs to be skiped before returning to aplication
        /// </summary>
        /// <returns></returns>
        private IEnumerator CompleteMethodInterstitial()
        {
            yield return null;
            if (OnInterstitialClosed != null)
            {
                OnInterstitialClosed();
                OnInterstitialClosed = null;
            }
            if (OnInterstitialClosedWithAdvertiser != null)
            {
                OnInterstitialClosedWithAdvertiser(SupportedAdvertisers.Admob.ToString());
                OnInterstitialClosedWithAdvertiser = null;
            }
        }

        /// <summary>
        /// Admob specific event triggered if an interstitial failed to load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterstitialFailed(object sender, AdFailedToLoadEventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Interstitial Failed To Load " + e.Message);
                ScreenWriter.Write(this + " " + "Interstitial Failed To Load " + e.Message);
            }
        }


        /// <summary>
        /// Loads an Admob rewarded video
        /// </summary>
        private void LoadRewardedVideo()
        {
            if (debug)
            {
                Debug.Log(this + " " + "Start Loading Rewarded Video");
                ScreenWriter.Write(this + " " + "Start Loading Rewarded Video");
            }

            AdRequest request = new AdRequest.Builder().AddExtra("npa", consent).AddExtra("is_designed_for_families", designedForFamilies).TagForChildDirectedTreatment(directedForChildren).Build();
            rewardedVideo.LoadAd(request, rewardedVideoId);
        }


        /// <summary>
        /// Admob specific event triggered when a rewarded video was skipped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAdClosed(object sender, EventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "OnAdClosed");
                ScreenWriter.Write(this + " " + "OnAdClosed");
            }

            //reload
            LoadRewardedVideo();

            rewardedVideoAdClosed = true;
#if !UNITY_ANDROID
            //if complete method was not already triggered, trigger complete method with ad skipped param
            if (triggerCompleteMethod == true)
            {
                StartCoroutine(CompleteMethodRewardedVideo(false));
            }
#endif
        }


        /// <summary>
        /// Because Admob has some problems when used in multithreading aplications with Unity a frame needs to be skiped before returning to aplication
        /// </summary>
        /// <returns></returns>
        private IEnumerator CompleteMethodRewardedVideo(bool val)
        {
            yield return null;
            if (OnCompleteMethod != null)
            {
                OnCompleteMethod(val);
                OnCompleteMethod = null;
            }
            if (OnCompleteMethodWithAdvertiser != null)
            {
                OnCompleteMethodWithAdvertiser(val, SupportedAdvertisers.Admob.ToString());
                OnCompleteMethodWithAdvertiser = null;
            }
        }


        /// <summary>
        /// Admob specific event triggered if a rewarded video failed to load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RewardedVideoFailed(object sender, AdFailedToLoadEventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Rewarded Video Failed " + e.Message);
                ScreenWriter.Write(this + " " + "Rewarded Video Failed " + e.Message);
            }

            //try again to load a rewarded video
            if (currentRetryRewardedVideo < maxRetryCount)
            {
                currentRetryRewardedVideo++;
                if (debug)
                {
                    Debug.Log(this + " " + "RETRY " + currentRetryRewardedVideo);
                    ScreenWriter.Write(this + " " + "RETRY " + currentRetryRewardedVideo);
                }
                LoadRewardedVideo();
            }
        }


        /// <summary>
        /// Admob specifie event triggered after a rewarded video was fully watched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RewardedVideoWatched(object sender, Reward e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "RewardedVideoWatched");
                ScreenWriter.Write(this + " " + "RewardedVideoWatched");
            }
            triggerCompleteMethod = false;
#if !UNITY_ANDROID
            StartCoroutine(CompleteMethodRewardedVideo(true));
#endif
        }


        /// <summary>
        /// Admob specific event triggered after a rewarded video is loaded and ready to be watched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RewardedVideoLoaded(object sender, EventArgs e)
        {
            if (debug)
            {
                Debug.Log(this + " " + "Rewarded Video Loaded");
                ScreenWriter.Write(this + " " + "Rewarded Video Loaded");
            }
            currentRetryRewardedVideo = 0;
        }


        /// <summary>
        /// Method triggered by Unity Engine when application is in focus.
        /// Because Admob uses multithreading, there are some errors when you create coroutine outside the main thread so we want to make sure the app is on main thread.
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
#if UNITY_ANDROID
            if (focus == true)
            {
                if (rewardedVideoAdClosed)
                {
                    rewardedVideoAdClosed = false;
                    if (triggerCompleteMethod == true)
                    {
                        StartCoroutine(CompleteMethodRewardedVideo(false));
                    }
                    else
                    {
                        StartCoroutine(CompleteMethodRewardedVideo(true));
                    }
                }

                if (interstitialAdCLosed)
                {
                    interstitialAdCLosed = false;
                    StartCoroutine(CompleteMethodInterstitial());
                }
            }
#endif
        }

#else
            //dummy interface implementation, used when Admob is not enabled
            public void InitializeAds(GDPRConsent consent, System.Collections.Generic.List<PlatformSettings> platformSettings)
        {

        }

        public bool IsInterstitialAvailable()
        {
            return false;
        }

        public bool IsRewardVideoAvailable()
        {
            return false;
        }

        public void ShowInterstitial(UnityAction InterstitialClosed = null)
        {

        }

        public void ShowInterstitial(UnityAction<string> InterstitialClosed)
        {

        }

        public void ShowRewardVideo(UnityAction<bool> CompleteMethod)
        {

        }

        public void HideBanner()
        {

        }

        public bool IsBannerAvailable()
        {
            return false;
        }

        public void ResetBannerUsage()
        {

        }

        public bool BannerAlreadyUsed()
        {
            return false;
        }

        public void ShowBanner(BannerPosition position, BannerType type, UnityAction<bool, BannerPosition, BannerType> DisplayResult)
        {
            
        }

        public void ShowRewardVideo(UnityAction<bool, string> CompleteMethod)
        {

        }

        public void UpdateConsent(GDPRConsent consent)
        {

        }

#endif
    }
}
