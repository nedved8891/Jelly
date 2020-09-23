using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class TestPurchase : MonoBehaviour
{
    void Start()
    {
        PurchaseManager.OnPurchaseNonConsumable += PurchaseManager_OnPurchaseNonConsumable;
    }

    private void PurchaseManager_OnPurchaseNonConsumable(PurchaseEventArgs args)
    {
        PlayerPrefs.SetInt("NoAds", 1);
        PlayerPrefs.Save();

        Debug.Log("You purchase: " + args.purchasedProduct.definition.id);
    }
}
