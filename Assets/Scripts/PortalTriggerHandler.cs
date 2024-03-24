using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PortalHandler.PortalEntered();
    }
}
