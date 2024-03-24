using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupHandler : MonoBehaviour
{
    [SerializeField] private AudioClip popClip = null;
    [SerializeField] private float sineOffsetStrength = 0.5f;
    [SerializeField] private float sineOffsetFrequency = 1f;

    private Vector3 originalPosition = Vector3.zero;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        transform.position = originalPosition + Vector3.up * Mathf.Sin(Time.time * sineOffsetFrequency) * sineOffsetStrength;
    }

    private void OnTriggerEnter(Collider other)
    {
        AudioSource.PlayClipAtPoint(popClip, transform.position);
        PlayerCharacterController.IncrementPickupNumber();
        Destroy(gameObject);
    }
}
