using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupHandler : MonoBehaviour
{
    [SerializeField] private AudioClip popClip = null;

    private void OnTriggerEnter(Collider other)
    {
        AudioSource.PlayClipAtPoint(popClip, transform.position);
        PlayerCharacterController.IncrementPickupNumber();
        Destroy(gameObject);
    }
}
