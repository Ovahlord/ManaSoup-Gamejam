using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float bodyTurnSpeed = 720f;

    private CharacterController characterController = null;
    private Vector2 moveInputValue = Vector2.zero;
    private Vector3 motion = Vector3.zero;
    private float targetBodyRotation = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        GameObject cameraFollower = new("CameraFollowe");
        PlayerCameraFollower follower = cameraFollower.AddComponent<PlayerCameraFollower>();
        follower.TargetTransform = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetBodyRotation, bodyTurnSpeed * Time.deltaTime), 0f);

        if (moveInputValue.magnitude == 0f)
            return;

        motion = new Vector3(moveInputValue.x, -1f, moveInputValue.y) * (Time.deltaTime * moveSpeed);
        characterController.Move(motion);
    }

    void OnMove(InputValue value)
    {
        moveInputValue = value.Get<Vector2>();
        if (moveInputValue.magnitude != 0f)
            targetBodyRotation = Mathf.Atan2(moveInputValue.x, moveInputValue.y) * Mathf.Rad2Deg;
    }
}
