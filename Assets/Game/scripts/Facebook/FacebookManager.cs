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
            FB.Init(InitCallback,OnHideUnity);
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
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
    }
}
