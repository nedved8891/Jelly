namespace GleyMobileAds
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using System.IO;
    using System.Collections.Generic;
    using UnityEditorInternal;
    using System.Linq;
    using System;

    [Serializable]
    public class EditorAdvertisers
    {
        public bool isActive;
        public SupportedAdvertisers advertiser;
        public AdTypeSettings adSettings;

        public EditorAdvertisers(SupportedAdvertisers advertiser, SupportedAdTypes adType, AdTypeSettings adSettings)
        {
            isActive = true;
            this.advertiser = advertiser;
            this.adSettings = adSettings;
        }
    }

    public class SettingsWindow : EditorWindow
    {
        Vector2 scrollPosition = Vector2.zero;
        AdSettings adSettings;
        List<AdvertiserSettings> advertiserSettings;
        List<MediationSettings> mediationSettings;

        private ReorderableList bannerList;
        public List<EditorAdvertisers> selectedBannerAdvertisers = new List<EditorAdvertisers>();

        private ReorderableList interstitialList;
        public List<EditorAdvertisers> selectedInterstitialAdvertisers = new List<EditorAdvertisers>();

        private ReorderableList rewardedList;
        public List<EditorAdvertisers> selectedRewardedAdvertisers = new List<EditorAdvertisers>();

        bool debugMode;
        bool usePlaymaker;

        SupportedMediation bannerMediation;
        SupportedMediation interstitialMediation;
        SupportedMediation rewardedMediation;

        string externalFileUrl;

        ScriptableObject target;
        SerializedObject so;

        [MenuItem("Window/Gley/Mobile Ads")]
        static void Init()
        {
            SettingsWindow window = (SettingsWindow)GetWindow(typeof(SettingsWindow), true, "Mobile Ads Settings Window - v.1.4.1");
            window.minSize = new Vector2(520, 520);
            window.Show();
        }

        private void OnEnable()
        {
            adSettings = Resources.Load<AdSettings>("AdSettingsData");
            if (adSettings == null)
            {
                CreateAdSettings();
                adSettings = Resources.Load<AdSettings>("AdSettingsData");
            }

            bannerMediation = adSettings.bannerMediation;
            interstitialMediation = adSettings.interstitialMediation;
            rewardedMediation = adSettings.rewardedMediation;

            advertiserSettings = new List<AdvertiserSettings>();
            for (int i = 0; i < adSettings.advertiserSettings.Count; i++)
            {
                advertiserSettings.Add(adSettings.advertiserSettings[i]);
            }
            UpdateAdvertiserSettings();

            mediationSettings = new List<MediationSettings>();
            for (int i = 0; i < adSettings.mediationSettings.Count; i++)
            {
                mediationSettings.Add(new MediationSettings(adSettings.mediationSettings[i].advertiser, adSettings.mediationSettings[i].bannerSettings, adSettings.mediationSettings[i].interstitialSettings, new AdTypeSettings(adSettings.mediationSettings[i].rewardedSettings)));
            }
            LoadMediationList();

            debugMode = adSettings.debugMode;
            usePlaymaker = adSettings.usePlaymaker;

            target = this;
            so = new SerializedObject(target);

            DrawBannerList();
            DrawInterstitialList();
            DrawRewardedList();

            externalFileUrl = adSettings.externalFileUrl;
        }


        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(position.width), GUILayout.Height(position.height));
            GUILayout.Label("Advertisement Settings", EditorStyles.boldLabel);
            GUILayout.Label("Select the ad providers you want to include:");

            debugMode = EditorGUILayout.Toggle("On Screen Debug Mode", debugMode);
            usePlaymaker = EditorGUILayout.Toggle("Playmaker Support", usePlaymaker);

            //show settings for all advertisers
            for (int i = 0; i < advertiserSettings.Count; i++)
            {
                string mediationText = " Ads";
                if (advertiserSettings[i].advertiser == SupportedAdvertisers.Admob || advertiserSettings[i].advertiser == SupportedAdvertisers.Heyzap)
                {
                    mediationText = " Ads - Supports mediation of all other ads";
                }

                advertiserSettings[i].useSDK = EditorGUILayout.BeginToggleGroup(advertiserSettings[i].advertiser + mediationText, advertiserSettings[i].useSDK);
                if (advertiserSettings[i].useSDK)
                {
                    for (int j = 0; j < advertiserSettings[i].platformSettings.Count; j++)
                    {
                        advertiserSettings[i].platformSettings[j].enabled = EditorGUILayout.Toggle(advertiserSettings[i].platformSettings[j].platform.ToString(), advertiserSettings[i].platformSettings[j].enabled);
                        if (advertiserSettings[i].platformSettings[j].enabled)
                        {
                            if (advertiserSettings[i].platformSettings[j].appId.notRequired == false || !String.IsNullOrEmpty(advertiserSettings[i].platformSettings[j].appId.displayName))
                            {
                                advertiserSettings[i].platformSettings[j].appId.id = EditorGUILayout.TextField(advertiserSettings[i].platformSettings[j].appId.displayName, advertiserSettings[i].platformSettings[j].appId.id);
                            }
                            if (advertiserSettings[i].platformSettings[j].hasBanner == true && !String.IsNullOrEmpty(advertiserSettings[i].platformSettings[j].idBanner.displayName))
                            {
                                advertiserSettings[i].platformSettings[j].idBanner.id = EditorGUILayout.TextField(advertiserSettings[i].platformSettings[j].idBanner.displayName, advertiserSettings[i].platformSettings[j].idBanner.id);
                            }
                            if (advertiserSettings[i].platformSettings[j].hasInterstitial == true && !String.IsNullOrEmpty(advertiserSettings[i].platformSettings[j].idInterstitial.displayName))
                            {
                                advertiserSettings[i].platformSettings[j].idInterstitial.id = EditorGUILayout.TextField(advertiserSettings[i].platformSettings[j].idInterstitial.displayName, advertiserSettings[i].platformSettings[j].idInterstitial.id);
                            }
                            if (advertiserSettings[i].platformSettings[j].hasRewarded == true && !String.IsNullOrEmpty(advertiserSettings[i].platformSettings[j].idRewarded.displayName))
                            {
                                advertiserSettings[i].platformSettings[j].idRewarded.id = EditorGUILayout.TextField(advertiserSettings[i].platformSettings[j].idRewarded.displayName, advertiserSettings[i].platformSettings[j].idRewarded.id);
                            }
                        }
                        if (advertiserSettings[i].advertiser == SupportedAdvertisers.Admob|| advertiserSettings[i].advertiser == SupportedAdvertisers.Heyzap|| advertiserSettings[i].advertiser == SupportedAdvertisers.AppLovin)
                        {
                            advertiserSettings[i].platformSettings[j].directedForChildren = EditorGUILayout.Toggle("Directed for children", advertiserSettings[i].platformSettings[j].directedForChildren);
                        }
                    }
                    if (GUILayout.Button("Download " + advertiserSettings[i].advertiser + " SDK"))
                    {
                        Application.OpenURL(advertiserSettings[i].sdkLink);
                    }
                }
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.Space();
            }

            so.Update();

            //mediation instructions
            if (selectedInterstitialAdvertisers.Count > 1)
            {
                EditorGUILayout.LabelField("Mediation Options", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Select your preferred mediation policy for each ad type.");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Order Mediation:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Order SDKs by dragging them in the list. The SDk on the top of the list is shown first. If no ad is available for the first SDK the next SDK will be shown. And so on.");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Percent Mediation:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("An add will be shown based on the percentages indicated next to the advertisers. A higher percentage means that a higher number of ads will be shown from that advertiser. Adjust the sliders until you reach the percentage that you think is best for your app.");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Unchecking the advertiser will not display any ad from that advertiser.");
                EditorGUILayout.Space();
            }

            //show banner mediation
            if (selectedBannerAdvertisers.Count > 1)
            {
                bannerMediation = (SupportedMediation)EditorGUILayout.EnumPopup("Banner Mediation: ", bannerMediation);
                bannerList.DoLayoutList();
                if (bannerMediation == SupportedMediation.PercentMediation)
                {
                    if (GUILayout.Button("OrderList"))
                    {
                        selectedBannerAdvertisers = selectedBannerAdvertisers.OrderByDescending(cond => cond.adSettings.Weight).ToList();
                    }
                }
                EditorGUILayout.Space();
            }

            //show interstitial mediation
            if (selectedInterstitialAdvertisers.Count > 1)
            {
                interstitialMediation = (SupportedMediation)EditorGUILayout.EnumPopup("Interstitial Mediation: ", interstitialMediation);
                interstitialList.DoLayoutList();
                if (interstitialMediation == SupportedMediation.PercentMediation)
                {
                    if (GUILayout.Button("OrderList"))
                    {
                        selectedInterstitialAdvertisers = selectedInterstitialAdvertisers.OrderByDescending(cond => cond.adSettings.Weight).ToList();
                    }
                }
                EditorGUILayout.Space();
            }

            //show rewarded mediation
            if (selectedRewardedAdvertisers.Count > 1)
            {
                rewardedMediation = (SupportedMediation)EditorGUILayout.EnumPopup("Rewarded Mediation: ", rewardedMediation);
                rewardedList.DoLayoutList();
                if (rewardedMediation == SupportedMediation.PercentMediation)
                {
                    if (GUILayout.Button("OrderList"))
                    {
                        selectedRewardedAdvertisers = selectedRewardedAdvertisers.OrderByDescending(cond => cond.adSettings.Weight).ToList();
                    }
                }
            }

            so.ApplyModifiedProperties();

            //apply changes
            UpdateMediationLists();
            selectedBannerAdvertisers = SetListValues(selectedBannerAdvertisers, bannerMediation);
            selectedInterstitialAdvertisers = SetListValues(selectedInterstitialAdvertisers, interstitialMediation);
            selectedRewardedAdvertisers = SetListValues(selectedRewardedAdvertisers, rewardedMediation);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //config file settings
            if (selectedInterstitialAdvertisers.Count > 1)
            {
                EditorGUILayout.LabelField("A config file can be generated then can be uploaded to an external server and the plugin will automatically read the mediation settings from that file. This is usefull to change the order of your ads without updating your build.");
                EditorGUILayout.LabelField("If you use Heyzap or Admob you can do exactly the same thing from their dashbord.");
                EditorGUILayout.Space();
                externalFileUrl = EditorGUILayout.TextField("External Settings File url", externalFileUrl);
                EditorGUILayout.Space();
                if (GUILayout.Button("Generate Settings File"))
                {
                    GenerateFile();
                }
            }

            //save settings
            EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
            {
                SaveSettings();
            }
            EditorGUILayout.Space();


            if (GUILayout.Button("Open Test Scene"))
            {
                EditorSceneManager.OpenScene("Assets/GleyPlugins/Ads/Example/TestAdsScene.unity");
            }

            GUILayout.EndScrollView();
        }

        private void SaveSettings()
        {
            SetPreprocessorDirectives();
            adSettings.debugMode = debugMode;
            adSettings.usePlaymaker = usePlaymaker;
            adSettings.bannerMediation = bannerMediation;
            adSettings.interstitialMediation = interstitialMediation;
            adSettings.rewardedMediation = rewardedMediation;

            adSettings.advertiserSettings = new List<AdvertiserSettings>();
            for (int i = 0; i < advertiserSettings.Count; i++)
            {
                adSettings.advertiserSettings.Add(advertiserSettings[i]);
            }

            adSettings.mediationSettings = new List<MediationSettings>();
            for (int i = 0; i < selectedBannerAdvertisers.Count; i++)
            {
                MediationSettings temp = new MediationSettings(selectedBannerAdvertisers[i].advertiser);
                temp.bannerSettings = new AdTypeSettings(selectedBannerAdvertisers[i].adSettings);
                adSettings.mediationSettings.Add(temp);
            }

            for (int i = 0; i < selectedInterstitialAdvertisers.Count; i++)
            {
                MediationSettings temp = adSettings.mediationSettings.FirstOrDefault(cond => cond.advertiser == selectedInterstitialAdvertisers[i].advertiser);
                if (temp == null)
                {
                    temp = new MediationSettings(selectedInterstitialAdvertisers[i].advertiser);
                    temp.interstitialSettings = new AdTypeSettings(selectedInterstitialAdvertisers[i].adSettings);
                    adSettings.mediationSettings.Add(temp);
                }
                else
                {
                    temp.interstitialSettings = new AdTypeSettings(selectedInterstitialAdvertisers[i].adSettings);
                }
            }

            for (int i = 0; i < selectedRewardedAdvertisers.Count; i++)
            {
                MediationSettings temp = adSettings.mediationSettings.FirstOrDefault(cond => cond.advertiser == selectedRewardedAdvertisers[i].advertiser);
                if (temp == null)
                {
                    temp = new MediationSettings(selectedRewardedAdvertisers[i].advertiser);
                    temp.rewardedSettings = new AdTypeSettings(selectedRewardedAdvertisers[i].adSettings);
                    adSettings.mediationSettings.Add(temp);
                }
                else
                {
                    temp.rewardedSettings = new AdTypeSettings(selectedRewardedAdvertisers[i].adSettings);
                }
            }

            adSettings.externalFileUrl = externalFileUrl;
            EditorUtility.SetDirty(adSettings);
        }


        void DrawBannerList()
        {
            bannerList = new ReorderableList(so, so.FindProperty("selectedBannerAdvertisers"), true, true, false, false);
            bannerList.drawHeaderCallback = rect =>
            {
                if (bannerMediation == SupportedMediation.OrderMediation)
                {
                    EditorGUI.LabelField(rect, "Banner Advertisers - Order Mediation");
                }
                else
                {
                    EditorGUI.LabelField(rect, "Banner Advertisers - Percent Mediation");
                }
            };

            bannerList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = bannerList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("isActive"), GUIContent.none);
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight), ((SupportedAdvertisers)element.FindPropertyRelative("advertiser").enumValueIndex).ToString());
                if (bannerMediation == SupportedMediation.OrderMediation)
                {
                    bannerList.draggable = true;
                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 120, EditorGUIUtility.singleLineHeight), "Order: " + selectedBannerAdvertisers[index].adSettings.Order.ToString());
                }
                else
                {
                    bannerList.draggable = false;
                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 55, EditorGUIUtility.singleLineHeight), selectedBannerAdvertisers[index].adSettings.Percent.ToString() + " %");
                    selectedBannerAdvertisers[index].adSettings.Weight = EditorGUI.IntSlider(new Rect(rect.x + 170, rect.y, 300, EditorGUIUtility.singleLineHeight), selectedBannerAdvertisers[index].adSettings.Weight, 1, 100);
                }
            };
        }


        void DrawInterstitialList()
        {
            interstitialList = new ReorderableList(so, so.FindProperty("selectedInterstitialAdvertisers"), true, true, false, false);
            interstitialList.drawHeaderCallback = rect =>
            {
                if (interstitialMediation == SupportedMediation.OrderMediation)
                {
                    EditorGUI.LabelField(rect, "Interstitial Advertisers - Order Mediation");
                }
                else
                {
                    EditorGUI.LabelField(rect, "Interstitial Advertisers - Percent Mediation");
                }
            };

            interstitialList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = interstitialList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("isActive"), GUIContent.none);
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight), ((SupportedAdvertisers)element.FindPropertyRelative("advertiser").enumValueIndex).ToString());
                if (interstitialMediation == SupportedMediation.OrderMediation)
                {
                    interstitialList.draggable = true;
                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 120, EditorGUIUtility.singleLineHeight), "Order: " + selectedInterstitialAdvertisers[index].adSettings.Order.ToString());
                }
                else
                {
                    interstitialList.draggable = false;
                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 55, EditorGUIUtility.singleLineHeight), selectedInterstitialAdvertisers[index].adSettings.Percent.ToString() + " %");
                    selectedInterstitialAdvertisers[index].adSettings.Weight = EditorGUI.IntSlider(new Rect(rect.x + 170, rect.y, 300, EditorGUIUtility.singleLineHeight), selectedInterstitialAdvertisers[index].adSettings.Weight, 1, 100);
                }
            };
        }


        void DrawRewardedList()
        {
            rewardedList = new ReorderableList(so, so.FindProperty("selectedRewardedAdvertisers"), true, true, false, false);
            rewardedList.drawHeaderCallback = rect =>
            {
                if (rewardedMediation == SupportedMediation.OrderMediation)
                {
                    EditorGUI.LabelField(rect, "Rewarded Video Advertisers - Order Mediation");
                }
                else
                {
                    EditorGUI.LabelField(rect, "Rewarded Video Advertisers - Percent Mediation");
                }
            };

            rewardedList.drawElementCallback =
           (Rect rect, int index, bool isActive, bool isFocused) =>
           {

               var element = rewardedList.serializedProperty.GetArrayElementAtIndex(index);
               rect.y += 2;
               EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("isActive"), GUIContent.none);
               EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight), ((SupportedAdvertisers)element.FindPropertyRelative("advertiser").enumValueIndex).ToString());
               if (rewardedMediation == SupportedMediation.OrderMediation)
               {
                   rewardedList.draggable = true;
                   EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 120, EditorGUIUtility.singleLineHeight), "Order: " + selectedRewardedAdvertisers[index].adSettings.Order.ToString());
               }
               else
               {
                   rewardedList.draggable = false;
                   EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 55, EditorGUIUtility.singleLineHeight), selectedRewardedAdvertisers[index].adSettings.Percent.ToString() + " %");
                   selectedRewardedAdvertisers[index].adSettings.Weight = EditorGUI.IntSlider(new Rect(rect.x + 170, rect.y, 300, EditorGUIUtility.singleLineHeight), selectedRewardedAdvertisers[index].adSettings.Weight, 1, 100);
               }
           };
        }

        //this function should be changed when new advertiser is added
        private void UpdateAdvertiserSettings()
        {
            Debug.Log("UpdateAdvertiserSettings");
            //AdColony
            if (advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.AdColony) == null)
            {
                AdvertiserSettings advertiser = new AdvertiserSettings(SupportedAdvertisers.AdColony, "https://github.com/AdColony/", "USE_ADCOLONY");
                advertiser.platformSettings = new List<PlatformSettings>
                {
                    new PlatformSettings(SupportedPlatforms.Android,new AdvertiserId("App ID"),new AdvertiserId(),new AdvertiserId("Interstitial Zone ID"),new AdvertiserId("Rewarded Zone ID"),false,true,true),
                    new PlatformSettings(SupportedPlatforms.iOS,new AdvertiserId("App ID"),new AdvertiserId(),new AdvertiserId("Interstitial Zone ID"),new AdvertiserId("Rewarded Zone ID"),false,true,true),
                };
                advertiserSettings.Add(advertiser);
            }
            //Admob
            if (advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.Admob) == null)
            {
                AdvertiserSettings advertiser = new AdvertiserSettings(SupportedAdvertisers.Admob, "https://github.com/googleads/googleads-mobile-unity/releases", "USE_ADMOB");
                advertiser.platformSettings = new List<PlatformSettings>
                {
                    new PlatformSettings(SupportedPlatforms.Android,new AdvertiserId("App ID"),new AdvertiserId("Banner ID"),new AdvertiserId("Interstitial ID"),new AdvertiserId("Rewarded Video ID"),true,true,true),
                    new PlatformSettings(SupportedPlatforms.iOS,new AdvertiserId("App ID"),new AdvertiserId("Banner ID"),new AdvertiserId("Interstitial ID"),new AdvertiserId("Rewarded Video ID"),true,true,true),
                };
                advertiserSettings.Add(advertiser);
            }
            else
            {
                //append appid support
                AdvertiserSettings advertiser = advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.Admob);
                for (int i = 0; i < advertiser.platformSettings.Count; i++)
                {
                    if (advertiser.platformSettings[i].appId.displayName == "")
                    {
                        advertiser.platformSettings[i].appId = new AdvertiserId("App ID");
                    }
                }
            }
            //Chartboost
            if (advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.Chartboost) == null)
            {
                AdvertiserSettings advertiser = new AdvertiserSettings(SupportedAdvertisers.Chartboost, "https://answers.chartboost.com/en-us/articles/download", "USE_CHARTBOOST");
                advertiser.platformSettings = new List<PlatformSettings>
                {
                    new PlatformSettings(SupportedPlatforms.Android,new AdvertiserId("App ID"),new AdvertiserId(),new AdvertiserId("App Signature"),new AdvertiserId(),false,true,true),
                    new PlatformSettings(SupportedPlatforms.iOS,new AdvertiserId("App ID"),new AdvertiserId(),new AdvertiserId("App Signature"),new AdvertiserId(),false,true,true),
                };
                advertiserSettings.Add(advertiser);
            }
            //Heyzap
            if (advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.Heyzap) == null)
            {
                AdvertiserSettings advertiser = new AdvertiserSettings(SupportedAdvertisers.Heyzap, "https://developers.heyzap.com/docs/unity_sdk_setup_and_requirements", "USE_HEYZAP");
                advertiser.platformSettings = new List<PlatformSettings>
                {
                    new PlatformSettings(SupportedPlatforms.Android,new AdvertiserId("Publisher ID"),new AdvertiserId(),new AdvertiserId(),new AdvertiserId(),true,true,true),
                    new PlatformSettings(SupportedPlatforms.iOS,new AdvertiserId("Publisher ID"),new AdvertiserId(),new AdvertiserId(),new AdvertiserId(),true,true,true),
                };
                advertiserSettings.Add(advertiser);
            }
            //UnityAds
            if (advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.Unity) == null)
            {
                AdvertiserSettings advertiser = new AdvertiserSettings(SupportedAdvertisers.Unity, "https://www.assetstore.unity3d.com/en/#!/content/66123", "USE_UNITYADS");
                advertiser.platformSettings = new List<PlatformSettings>
                {
                    new PlatformSettings(SupportedPlatforms.Android,new AdvertiserId("Game ID"),new AdvertiserId("Placement ID Banner"),new AdvertiserId("Placement ID Interstitial"),new AdvertiserId("Placement ID Rewarded"),true,true,true),
                    new PlatformSettings(SupportedPlatforms.iOS,new AdvertiserId("Game ID"),new AdvertiserId("Placement ID Banner"),new AdvertiserId("Placement ID Interstitial"),new AdvertiserId("Placement ID Rewarded"),true,true,true),
                };
                advertiserSettings.Add(advertiser);
            }
            else
            {
                //append banner support
                AdvertiserSettings advertiser = advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.Unity);
                for (int i = 0; i < advertiser.platformSettings.Count; i++)
                {
                    if (advertiser.platformSettings[i].hasBanner == false)
                    {
                        advertiser.platformSettings[i].hasBanner = true;
                        advertiser.platformSettings[i].idBanner = new AdvertiserId("Placement ID Banner");
                    }
                }
            }
            //Vungle
            if (advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.Vungle) == null)
            {
                AdvertiserSettings advertiser = new AdvertiserSettings(SupportedAdvertisers.Vungle, "https://v.vungle.com/sdk", "USE_VUNGLE");
                advertiser.platformSettings = new List<PlatformSettings>
                {
                    new PlatformSettings(SupportedPlatforms.Android,new AdvertiserId("App ID"),new AdvertiserId(),new AdvertiserId("Interstitial Placement ID"),new AdvertiserId("Rewarded Placement ID"),false,true,true),
                    new PlatformSettings(SupportedPlatforms.iOS,new AdvertiserId("App ID"),new AdvertiserId(),new AdvertiserId("Interstitial Placement ID"),new AdvertiserId("Rewarded Placement ID"),false,true,true),
                    new PlatformSettings(SupportedPlatforms.Windows,new AdvertiserId("App ID"),new AdvertiserId(),new AdvertiserId("Interstitial Placement ID"),new AdvertiserId("Rewarded Placement ID"),false,true,true),
                };
                advertiserSettings.Add(advertiser);
            }
            //AppLovin
            if(advertiserSettings.Find(cond => cond.advertiser == SupportedAdvertisers.AppLovin)==null)
            {
                AdvertiserSettings advertiser = new AdvertiserSettings(SupportedAdvertisers.AppLovin, "https://dash.applovin.com/docs/integration#unity3dIntegration", "USE_APPLOVIN");
                advertiser.platformSettings = new List<PlatformSettings>
                {
                    new PlatformSettings(SupportedPlatforms.Android,new AdvertiserId("SDK key"),new AdvertiserId(),new AdvertiserId(),new AdvertiserId(),true,true,true),
                    new PlatformSettings(SupportedPlatforms.iOS,new AdvertiserId("SDK key"),new AdvertiserId(),new AdvertiserId(),new AdvertiserId(),true,true,true)
                };
                advertiserSettings.Add(advertiser);
            }
        }


        private void LoadMediationList()
        {
            selectedBannerAdvertisers = new List<EditorAdvertisers>();
            selectedInterstitialAdvertisers = new List<EditorAdvertisers>();
            selectedRewardedAdvertisers = new List<EditorAdvertisers>();
            for (int i = 0; i < mediationSettings.Count; i++)
            {
                if (mediationSettings[i].bannerSettings != null && mediationSettings[i].bannerSettings.adType != SupportedAdTypes.None)
                {
                    EditorAdvertisers editorAdvertiser = new EditorAdvertisers(mediationSettings[i].advertiser, SupportedAdTypes.Banner, mediationSettings[i].bannerSettings);
                    if (bannerMediation == SupportedMediation.PercentMediation)
                    {
                        if (mediationSettings[i].bannerSettings.Weight == 0)
                        {
                            editorAdvertiser.isActive = false;
                        }
                    }
                    else
                    {
                        if (mediationSettings[i].bannerSettings.Order == 0)
                        {
                            editorAdvertiser.isActive = false;
                        }
                    }
                    selectedBannerAdvertisers.Add(editorAdvertiser);
                }

                if (mediationSettings[i].interstitialSettings != null && mediationSettings[i].interstitialSettings.adType != SupportedAdTypes.None)
                {
                    EditorAdvertisers editorAdvertiser = new EditorAdvertisers(mediationSettings[i].advertiser, SupportedAdTypes.Interstitial, mediationSettings[i].interstitialSettings);
                    if (interstitialMediation == SupportedMediation.PercentMediation)
                    {
                        if (mediationSettings[i].interstitialSettings.Weight == 0)
                        {
                            editorAdvertiser.isActive = false;
                        }
                    }
                    else
                    {
                        if (mediationSettings[i].interstitialSettings.Order == 0)
                        {
                            editorAdvertiser.isActive = false;
                        }
                    }
                    selectedInterstitialAdvertisers.Add(editorAdvertiser);
                }

                if (mediationSettings[i].rewardedSettings != null && mediationSettings[i].rewardedSettings.adType != SupportedAdTypes.None)
                {
                    EditorAdvertisers editorAdvertiser = new EditorAdvertisers(mediationSettings[i].advertiser, SupportedAdTypes.Rewarded, mediationSettings[i].rewardedSettings);
                    if (rewardedMediation == SupportedMediation.PercentMediation)
                    {
                        if (mediationSettings[i].rewardedSettings.Weight == 0)
                        {
                            editorAdvertiser.isActive = false;
                        }
                    }
                    else
                    {
                        if (mediationSettings[i].rewardedSettings.Order == 0)
                        {
                            editorAdvertiser.isActive = false;
                        }
                    }
                    selectedRewardedAdvertisers.Add(editorAdvertiser);
                }
            }

            if (bannerMediation == SupportedMediation.PercentMediation)
            {
                selectedBannerAdvertisers = selectedBannerAdvertisers.OrderByDescending(cond => cond.adSettings.Weight).ToList();
            }
            else
            {
                selectedBannerAdvertisers = selectedBannerAdvertisers.OrderBy(cond => cond.adSettings.Order).ToList();
            }

            if (interstitialMediation == SupportedMediation.PercentMediation)
            {
                selectedInterstitialAdvertisers = selectedInterstitialAdvertisers.OrderByDescending(cond => cond.adSettings.Weight).ToList();
            }
            else
            {
                selectedInterstitialAdvertisers = selectedInterstitialAdvertisers.OrderBy(cond => cond.adSettings.Order).ToList();
            }

            if (rewardedMediation == SupportedMediation.PercentMediation)
            {
                selectedRewardedAdvertisers = selectedRewardedAdvertisers.OrderByDescending(cond => cond.adSettings.Weight).ToList();
            }
            else
            {
                selectedRewardedAdvertisers = selectedRewardedAdvertisers.OrderBy(cond => cond.adSettings.Order).ToList();
            }
        }

        private void UpdateMediationLists()
        {
            for (int i = 0; i < advertiserSettings.Count; i++)
            {
                if (advertiserSettings[i].useSDK == true)
                {
                    if (advertiserSettings[i].platformSettings[0].hasBanner)
                    {
                        EditorAdvertisers advertiser = selectedBannerAdvertisers.FirstOrDefault(cond => cond.advertiser == advertiserSettings[i].advertiser);

                        if (advertiser == null)
                        {
                            selectedBannerAdvertisers.Add(new EditorAdvertisers(advertiserSettings[i].advertiser, SupportedAdTypes.Banner, new AdTypeSettings(SupportedAdTypes.Banner)));
                        }
                    }
                    if (advertiserSettings[i].platformSettings[0].hasInterstitial)
                    {
                        EditorAdvertisers advertiser = selectedInterstitialAdvertisers.FirstOrDefault(cond => cond.advertiser == advertiserSettings[i].advertiser);
                        if (advertiser == null)
                        {
                            selectedInterstitialAdvertisers.Add(new EditorAdvertisers(advertiserSettings[i].advertiser, SupportedAdTypes.Interstitial, new AdTypeSettings(SupportedAdTypes.Interstitial)));
                        }
                    }
                    if (advertiserSettings[i].platformSettings[0].hasRewarded)
                    {
                        EditorAdvertisers advertiser = selectedRewardedAdvertisers.FirstOrDefault(cond => cond.advertiser == advertiserSettings[i].advertiser);
                        if (advertiser == null)
                        {
                            selectedRewardedAdvertisers.Add(new EditorAdvertisers(advertiserSettings[i].advertiser, SupportedAdTypes.Rewarded, new AdTypeSettings(SupportedAdTypes.Rewarded)));
                        }
                    }
                }
                else
                {
                    if (advertiserSettings[i].platformSettings[0].hasBanner)
                    {
                        EditorAdvertisers advertiser = selectedBannerAdvertisers.FirstOrDefault(cond => cond.advertiser == advertiserSettings[i].advertiser);
                        if (advertiser != null)
                        {
                            selectedBannerAdvertisers.Remove(advertiser);
                        }
                    }
                    if (advertiserSettings[i].platformSettings[0].hasInterstitial)
                    {
                        EditorAdvertisers advertiser = selectedInterstitialAdvertisers.FirstOrDefault(cond => cond.advertiser == advertiserSettings[i].advertiser);
                        if (advertiser != null)
                        {
                            selectedInterstitialAdvertisers.Remove(advertiser);
                        }
                    }
                    if (advertiserSettings[i].platformSettings[0].hasRewarded)
                    {
                        EditorAdvertisers advertiser = selectedRewardedAdvertisers.FirstOrDefault(cond => cond.advertiser == advertiserSettings[i].advertiser);
                        if (advertiser != null)
                        {
                            selectedRewardedAdvertisers.Remove(advertiser);
                        }
                    }
                }
            }
        }


        private List<EditorAdvertisers> SetListValues(List<EditorAdvertisers> advertiserList, SupportedMediation mediation)
        {
            if (advertiserList.Count == 1)
            {
                advertiserList[0].adSettings.Order = 1;
                advertiserList[0].adSettings.Weight = 1;
                advertiserList[0].adSettings.Percent = 100;
                return advertiserList;
            }
            for (int i = advertiserList.Count - 1; i >= 0; i--)
            {
                if (mediation == SupportedMediation.OrderMediation)
                {
                    if (advertiserList[i].isActive)
                    {
                        advertiserList[i].adSettings.Order = i + 1;
                    }
                    else
                    {
                        advertiserList[i].adSettings.Order = 0;
                        EditorAdvertisers item = advertiserList[i];
                        advertiserList.RemoveAt(i);
                        advertiserList.Add(item);
                    }
                }
                else
                {
                    if (advertiserList[i].isActive)
                    {
                        advertiserList[i].adSettings.Percent = (ConvertWeightToPercent(advertiserList[i].adSettings.Weight, advertiserList));
                    }
                    else
                    {
                        advertiserList[i].adSettings.Weight = 0;
                        advertiserList[i].adSettings.Percent = 0;
                        EditorAdvertisers item = advertiserList[i];
                        advertiserList.RemoveAt(i);
                        advertiserList.Add(item);
                    }
                }
            }
            return advertiserList;
        }


        public static void CreateAdSettings()
        {
            AdSettings asset = ScriptableObject.CreateInstance<AdSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/GleyPlugins/Ads/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/GleyPlugins/Ads", "Resources");
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(asset, "Assets/GleyPlugins/Ads/Resources/AdSettingsData.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private void GenerateFile()
        {
            AdOrder adOrder = new AdOrder();
            adOrder.bannerMediation = bannerMediation;
            adOrder.interstitialMediation = interstitialMediation;
            adOrder.rewardedMediation = rewardedMediation;
            adOrder.advertisers = adSettings.mediationSettings;
            string json = JsonUtility.ToJson(adOrder);
            File.WriteAllText(Application.dataPath + "/GleyPlugins/Ads/AdOrderFile/AdOrder.txt", json);
            AssetDatabase.Refresh();
        }


        private int ConvertWeightToPercent(float weight, List<EditorAdvertisers> advertisers)
        {
            float sum = 0;
            for (int i = 0; i < advertisers.Count; i++)
            {
                if (advertisers[i].isActive)
                {
                    sum += advertisers[i].adSettings.Weight;
                }
            }
            return (int)(weight * 100 / sum);
        }


        private void SetPreprocessorDirectives()
        {
            if(usePlaymaker)
            {
                AddPreprocessorDirective("USE_PLAYMAKER_SUPPORT", false, BuildTargetGroup.Android);
                AddPreprocessorDirective("USE_PLAYMAKER_SUPPORT", false, BuildTargetGroup.iOS);
                AddPreprocessorDirective("USE_PLAYMAKER_SUPPORT", false, BuildTargetGroup.WSA);
            }
            else
            {
                AddPreprocessorDirective("USE_PLAYMAKER_SUPPORT", true, BuildTargetGroup.Android);
                AddPreprocessorDirective("USE_PLAYMAKER_SUPPORT", true, BuildTargetGroup.iOS);
                AddPreprocessorDirective("USE_PLAYMAKER_SUPPORT", true, BuildTargetGroup.WSA);
            }

            for (int i = 0; i < advertiserSettings.Count; i++)
            {
                if (advertiserSettings[i].useSDK == true)
                {
                    if (advertiserSettings[i].advertiser == SupportedAdvertisers.Admob)
                    {
                        CreateManifestFile(advertiserSettings[i].platformSettings[(int)SupportedPlatforms.Android].appId.id);
                    }
                    for (int j = 0; j < advertiserSettings[i].platformSettings.Count; j++)
                    {
                        if (advertiserSettings[i].platformSettings[j].enabled == true)
                        {
                            if (advertiserSettings[i].platformSettings[j].platform == SupportedPlatforms.Android)
                            {
                                AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, false, BuildTargetGroup.Android);
                            }
                            if (advertiserSettings[i].platformSettings[j].platform == SupportedPlatforms.iOS)
                            {
                                AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, false, BuildTargetGroup.iOS);
                            }
                            if (advertiserSettings[i].platformSettings[j].platform == SupportedPlatforms.Windows)
                            {
                                AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, false, BuildTargetGroup.WSA);
                            }
                        }
                        else
                        {
                            if (advertiserSettings[i].platformSettings[j].platform == SupportedPlatforms.Android)
                            {
                                AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, true, BuildTargetGroup.Android);
                            }
                            if (advertiserSettings[i].platformSettings[j].platform == SupportedPlatforms.iOS)
                            {
                                AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, true, BuildTargetGroup.iOS);
                            }
                            if (advertiserSettings[i].platformSettings[j].platform == SupportedPlatforms.Windows)
                            {
                                AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, true, BuildTargetGroup.WSA);
                            }
                        }
                    }
                }
                else
                {
                    AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, true, BuildTargetGroup.Android);
                    AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, true, BuildTargetGroup.iOS);
                    AddPreprocessorDirective(advertiserSettings[i].preprocessorDirective, true, BuildTargetGroup.WSA);
                    if (advertiserSettings[i].advertiser == SupportedAdvertisers.Admob)
                    {
                        DisableManifest();
                    }
                }
            }
        }

     

        void AddPreprocessorDirective(string directive, bool remove, BuildTargetGroup target)
        {
            string textToWrite = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

            if (remove)
            {
                if (textToWrite.Contains(directive))
                {
                    textToWrite = textToWrite.Replace(directive, "");
                }
            }
            else
            {
                if (!textToWrite.Contains(directive))
                {
                    if (textToWrite == "")
                    {
                        textToWrite += directive;
                    }
                    else
                    {
                        textToWrite += "," + directive;
                    }
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, textToWrite);
        }

        /// <summary>
        /// Autogenerate Google Play manifest to replace the one generated by google
        /// </summary>
        void CreateManifestFile(string appId)
        {
            if (!AssetDatabase.IsValidFolder("Assets/GleyPlugins/Ads/Plugins"))
            {
                AssetDatabase.CreateFolder("Assets/GleyPlugins/Ads", "Plugins");
                AssetDatabase.Refresh();
                AssetDatabase.CreateFolder("Assets/GleyPlugins/Ads/Plugins", "Android");
                AssetDatabase.Refresh();
                AssetDatabase.CreateFolder("Assets/GleyPlugins/Ads/Plugins/Android", "GleyMobileAdsManifest.plugin");
                AssetDatabase.Refresh();
            }


            string text = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<manifest xmlns:android = \"http://schemas.android.com/apk/res/android\"\n" +
                "package=\"com.gley.mobileads\">\n" +
                "<application>\n" +
                "<meta-data android:name = \"com.google.android.gms.ads.APPLICATION_ID\" android:value = \""+appId+"\" />\n" +
                "</application>\n" +
                "</manifest>";

            File.WriteAllText(Application.dataPath + "/GleyPlugins/Ads/Plugins/Android/GleyMobileAdsManifest.plugin/AndroidManifest.xml", text);

            text = "target=android-16\nandroid.library = true";
            File.WriteAllText(Application.dataPath + "/GleyPlugins/Ads/Plugins/Android/GleyMobileAdsManifest.plugin/project.properties", text);
            AssetDatabase.Refresh();

            ((PluginImporter)PluginImporter.GetAtPath("Assets/GleyPlugins/Ads/Plugins/Android/GleyMobileAdsManifest.plugin")).SetCompatibleWithAnyPlatform(false);
            ((PluginImporter)PluginImporter.GetAtPath("Assets/GleyPlugins/Ads/Plugins/Android/GleyMobileAdsManifest.plugin")).SetCompatibleWithPlatform(BuildTarget.Android, true);
            AssetDatabase.Refresh();
        }

        private void DisableManifest()
        {
            if (AssetDatabase.IsValidFolder("Assets/GleyPlugins/Ads/Plugins"))
            {
                ((PluginImporter)PluginImporter.GetAtPath("Assets/GleyPlugins/Ads/Plugins/Android/GleyMobileAdsManifest.plugin")).SetCompatibleWithAnyPlatform(false);
                ((PluginImporter)PluginImporter.GetAtPath("Assets/GleyPlugins/Ads/Plugins/Android/GleyMobileAdsManifest.plugin")).SetCompatibleWithPlatform(BuildTarget.Android, false);
                AssetDatabase.Refresh();
            }
        }
    }
}