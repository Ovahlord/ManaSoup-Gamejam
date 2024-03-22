using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float bodyTurnSpeed = 720f;
    [SerializeField] private float cameraFollowLerpFactor = 5.0f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraFacingHeightOffset = 1f;
    [SerializeField] private List<GameObject> splittingCharacters = new List<GameObject>();

    private CharacterController characterController = null;
    private Vector2 moveInputValue = Vector2.zero;
    private Vector3 motion = Vector3.zero;
    private float targetBodyRotation = 0f;
    private List<GameObject> splittedCharacters = new List<GameObject>();

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        new GameObject("CameraFollower").AddComponent<PlayerCameraFollower>();

        PlayerCameraFollower.Target = transform;
        PlayerCameraFollower.CameraDistance = cameraDistance;
        PlayerCameraFollower.FollowLerpFactor = cameraFollowLerpFactor;
        PlayerCameraFollower.CameraFacingHeightOffset = cameraFacingHeightOffset;
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

    public void OnMove(InputValue value)
    {
        Debug.Log("OnMove has been invoked :)");

        moveInputValue = value.Get<Vector2>();
        if (moveInputValue.magnitude != 0f)
            targetBodyRotation = Mathf.Atan2(moveInputValue.x, moveInputValue.y) * Mathf.Rad2Deg;
    }

    public void OnSplit(InputValue value)
    {
        Debug.Log("splitting...");

        // We already have active splitted characters. Destroy them...
        if (splittedCharacters.Count > 0)
        {
            foreach (GameObject character in splittedCharacters)
                Destroy(character);
            return;
        }

        //... otherwise spawn new
        foreach (GameObject splittingCharacter in splittingCharacters)
            SpawnCharacter(splittingCharacter);
    }

    private void SpawnCharacter(GameObject character)
    {

    }
}
