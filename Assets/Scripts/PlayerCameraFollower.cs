using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraFollower : MonoBehaviour
{
    private float followLerpFactor = 5.0f;
    private float cameraDistance = 10f;
    private float cameraHeightFacingOffset = 1f;
    private Vector3 cameraOffset = Vector3.zero;
    private Vector3 heightFacingOffset = Vector3.zero;
    private Transform targeTransform = null;

    private static PlayerCameraFollower instance = null;

    private void Awake()
    {
        cameraOffset = Quaternion.Euler(45f, 0f, 0f) * (Vector3.back * cameraDistance);
        heightFacingOffset = new(0f, cameraHeightFacingOffset, 0f);

        if (instance != null)
            Destroy(instance);

        instance = this;
    }

    void LateUpdate()
    {
        if (Target == null)
            return;

        transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * FollowLerpFactor);
        Camera.main.transform.position = transform.position + cameraOffset;
        Camera.main.transform.LookAt(transform.position + heightFacingOffset);
    }

    public static Transform Target
    {
        get { return instance.targeTransform; }
        set { instance.targeTransform = value; }
    }

    public static float FollowLerpFactor
    {
        get { return instance.followLerpFactor; }
        set { instance.followLerpFactor = value; }
    }

    public static float CameraDistance
    {
        get { return instance.cameraDistance; }
        set { instance.cameraDistance = value; }
    }

    public static float CameraFacingHeightOffset
    {
        get { return instance.cameraHeightFacingOffset; }
        set
        {
            instance.cameraHeightFacingOffset = value;
            instance.heightFacingOffset = new(0f, value, 0f);
        }
    }
}
