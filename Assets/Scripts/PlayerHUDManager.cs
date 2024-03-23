using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    [SerializeField] private TMP_Text pickupInfoText = null;
    private static PlayerHUDManager instance = null;


    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(instance);

        instance = this;

    }


    public static void SetPickupValues(int required, int gatheredPickups)
    {
        instance.pickupInfoText.text = $"{ gatheredPickups } / { required }";
    }
}
