using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

public class FacebookManager : MonoBehaviour
{
    void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();

            if (PlayerPrefs.GetInt("FirstRunApp", 0) == 0)
            {
                PlayerPrefs.SetInt("FirstRunApp", 1);
                FB.Mobile.FetchDeferredAppLinkData(result =>
                {
                    AppMetrica.Instance.ReportReferralUrl(result.TargetUrl);
                    // Process app link data.
                });
            }
            else
            {
                FB.GetAppLink(result =>
                {
                    AppMetrica.Instance.ReportAppOpen(result.TargetUrl);
                    // Process app link data
                    FB.ClearAppLink();
                });
            }
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
    }
}
