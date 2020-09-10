using UnityEngine;

public class InitializeGleyAds : MonoBehaviour
{
    private void Start()
    {
        Advertisements.Instance.Initialize();
    }
}
