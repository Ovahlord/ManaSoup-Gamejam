using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    // Exposed Settings
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float bodyTurnSpeed = 720f;
    [SerializeField] private float cameraFollowLerpFactor = 5.0f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraFacingHeightOffset = 1f;
    [SerializeField] private List<GameObject> splittingCharacterPrefabs = new List<GameObject>();


    private CharacterController activePlayerCharacterController = null;
    private Vector2 moveInputValue = Vector2.zero;
    private Vector3 motion = Vector3.zero;
    private float targetBodyRotation = 0f;
    private List<GameObject> splittedCharacters = new List<GameObject>();
    private int controlledSplittedCharacterIndex = 0;

    private void Awake()
    {
        activePlayerCharacterController = GetComponent<CharacterController>();
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
        activePlayerCharacterController.transform.rotation = Quaternion.Euler(0f, Mathf.MoveTowardsAngle(activePlayerCharacterController.transform.eulerAngles.y, targetBodyRotation, bodyTurnSpeed * Time.deltaTime), 0f);

        if (moveInputValue.magnitude == 0f)
            return;

        motion = new Vector3(moveInputValue.x, -1f, moveInputValue.y) * (Time.deltaTime * moveSpeed);
        activePlayerCharacterController.Move(motion);
    }

    public void OnMove(InputValue value)
    {
        moveInputValue = value.Get<Vector2>();
        if (moveInputValue.magnitude != 0f)
            targetBodyRotation = Mathf.Atan2(moveInputValue.x, moveInputValue.y) * Mathf.Rad2Deg;
    }

    public void OnSplit(InputValue value)
    {
        if (splittingCharacterPrefabs.Count == 0)
            return;

        if (splittedCharacters.Count == 0)
        {
            for (int i = 0; i < splittingCharacterPrefabs.Count; ++i)
                splittedCharacters.Add(Instantiate(splittingCharacterPrefabs[i], transform.position + (i % 2 != 0 ? (Vector3.left * 1.2f) : (Vector3.right * 1.2f)), transform.rotation, null));

            controlledSplittedCharacterIndex = 0;
            activePlayerCharacterController = splittedCharacters[controlledSplittedCharacterIndex].transform.GetComponent<CharacterController>();
            PlayerCameraFollower.Target = activePlayerCharacterController.transform;
        }
        else
        {
            activePlayerCharacterController = GetComponent<CharacterController>();
            PlayerCameraFollower.Target = transform;

            foreach (var splittedCharacter in splittedCharacters)
                Destroy(splittedCharacter);

            splittedCharacters.Clear();
        }
    }

    public void OnSwitchCharacter(InputValue value)
    {
        controlledSplittedCharacterIndex = (controlledSplittedCharacterIndex + 1) % splittedCharacters.Count;
        activePlayerCharacterController = splittedCharacters[controlledSplittedCharacterIndex].transform.GetComponent<CharacterController>();
        PlayerCameraFollower.Target = activePlayerCharacterController.transform;
    }
}
