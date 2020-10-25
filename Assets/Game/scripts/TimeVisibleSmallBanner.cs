using System.Collections;
using System.Collections.Generic;
using AdColony;
using BSH_Prototype;
using UnityEngine;

public class TimeVisibleSmallBanner : MonoBehaviour
{
    public float lastTime;

    public float delayShow = 120;

    public float deleyHide = 10;

    public bool visible;

    private void Start()
    {
        lastTime = -delayShow;
    }

    // Update is called once per frame
    void Update()
    {
        if (!visible)
        {
            if (Time.time - lastTime > delayShow)
            {
                if(SceneLoader.Instance)
                    SceneLoader.Instance.CheckSmallBanner(SceneLoader.Instance.GetCurrentScene());

                lastTime = Time.time;

                visible = true;
            }
        }else
        {
            if (Time.time - lastTime > deleyHide)
            {
                if(AdsProvider.Instance)
                    AdsProvider.Instance.HideBannerAd();

                lastTime = Time.time;
                
                visible = false;
            }
        }
    }
}
