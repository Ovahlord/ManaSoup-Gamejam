using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraFollower : MonoBehaviour
{
    [SerializeField] private float followLerpSpeed = 5.0f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeightFacingOffset = 1f;
    public Transform TargetTransform = null;

    private Vector3 cameraOffset = Vector3.zero;
    private Vector3 heightFacingOffset = Vector3.zero;

    private void Awake()
    {
        cameraOffset = Quaternion.Euler(45f, 0f, 0f) * (Vector3.back * cameraDistance);
        heightFacingOffset = new(0f, cameraHeightFacingOffset, 0f);
    }

    void Update()
    {
        if (TargetTransform == null)
            return;

        transform.position = Vector3.Lerp(transform.position, TargetTransform.position, Time.deltaTime * followLerpSpeed);
        Camera.main.transform.position = transform.position + cameraOffset;
        Camera.main.transform.LookAt(transform.position + heightFacingOffset);
    }
}
