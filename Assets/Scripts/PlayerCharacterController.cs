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
    [SerializeField] private GameObject playerHudPrefab = null;
    [SerializeField] private ParticleSystem splitChannelParticleSystem = null;

    [Header("Jump Character Settings")]
    [SerializeField] private float MaxJumpHeight = 5f;
    [SerializeField] private float Gravity = 40f;

    private CharacterController activePlayerCharacterController = null;
    private Vector2 moveInputValue = Vector2.zero;
    private Vector3 motion = Vector3.zero;
    private float targetBodyRotation = 0f;
    private List<GameObject> splittedCharacters = new List<GameObject>();
    private int controlledSplittedCharacterIndex = 0;
    private float? verticalAcceleration = null;

    // Grabbing
    private Transform grabbedObject = null;
    private Vector3 previousPosition = Vector3.zero;
    private Vector3 previousGrabObjectPosition = Vector3.zero;
    private Collider[] colliderCache = new Collider[20];

    private readonly Vector3[] splittingRaycastDirections = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back, new(1f, 0f, 1f), new(-1f, 0f, -1f), new(1f, 0f, -1f), new(-1f, 0f, 1f) };

    private void Awake()
    {
        activePlayerCharacterController = GetComponent<CharacterController>();
        new GameObject("CameraFollower").AddComponent<PlayerCameraFollower>();

        PlayerCameraFollower.Target = transform;
        PlayerCameraFollower.CameraDistance = cameraDistance;
        PlayerCameraFollower.FollowLerpFactor = cameraFollowLerpFactor;
        PlayerCameraFollower.CameraFacingHeightOffset = cameraFacingHeightOffset;

        previousPosition = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(playerHudPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        if (grabbedObject == null)
            activePlayerCharacterController.transform.rotation = Quaternion.Euler(0f, Mathf.MoveTowardsAngle(activePlayerCharacterController.transform.eulerAngles.y, targetBodyRotation, bodyTurnSpeed * Time.deltaTime), 0f);

        if (moveInputValue.magnitude == 0f && !verticalAcceleration.HasValue)
            return;

        if (verticalAcceleration.HasValue)
        {
            verticalAcceleration -= Gravity * Time.deltaTime;
            if (verticalAcceleration.Value < -20f)
                verticalAcceleration = -20f;
        }

        float speed = moveSpeed;
        motion = new Vector3(moveInputValue.x * speed, verticalAcceleration ?? - 1f, moveInputValue.y * speed) * Time.deltaTime;
        CollisionFlags flags = activePlayerCharacterController.Move(motion);
        if (flags.HasFlag(CollisionFlags.CollidedBelow))
            verticalAcceleration = null;
        else if (!verticalAcceleration.HasValue)
            verticalAcceleration = 0f;

        UpdateGrabCollision();
    }

    private void LateUpdate()
    {
        previousPosition = transform.position;
        if (grabbedObject != null)
            previousGrabObjectPosition = grabbedObject.position;
    }

    public void OnMove(InputValue value)
    {
        moveInputValue = value.Get<Vector2>();
        if (moveInputValue.magnitude != 0f)
            targetBodyRotation = Mathf.Atan2(moveInputValue.x, moveInputValue.y) * Mathf.Rad2Deg;
    }

    public void OnSplit(InputValue value)
    {
        // We cannot split/unsplit just yet while we are still perform a jump
        if (verticalAcceleration.HasValue)
            return;

        if (splittingCharacterPrefabs.Count == 0)
            return;

        if (splittedCharacters.Count == 0)
        {
            for (int i = 0; i < splittingCharacterPrefabs.Count; ++i)
            {
                bool successfullySpawned = false;
                for (int j = i; j < splittingRaycastDirections.Length; ++j)
                {
                    if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, splittingRaycastDirections[j], 1.7f))
                    {
                        splittedCharacters.Add(Instantiate(splittingCharacterPrefabs[i], (transform.position + splittingRaycastDirections[j].normalized * 1.2f), transform.rotation, null));
                        successfullySpawned = true;
                        break;
                    }
                }

                if (!successfullySpawned)
                    break;
            }

            if (splittedCharacters.Count == 0)
                return;

            foreach (GameObject splittedCharacter in splittedCharacters)
            {
                if (Physics.Raycast(splittedCharacter.transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 10f))
                {
                    splittedCharacter.transform.position = hit.point;
                    if (hit.transform.CompareTag("MovableObject"))
                        splittedCharacter.transform.SetParent(hit.transform);
                }
            }

            controlledSplittedCharacterIndex = 0;
            activePlayerCharacterController = splittedCharacters[controlledSplittedCharacterIndex].transform.GetComponent<CharacterController>();
            PlayerCameraFollower.Target = activePlayerCharacterController.transform;

            splitChannelParticleSystem.Play();
        }
        else
        {
            activePlayerCharacterController = GetComponent<CharacterController>();
            PlayerCameraFollower.Target = transform;

            ReleaseGrabbedObject();

            foreach (var splittedCharacter in splittedCharacters)
                Destroy(splittedCharacter);

            splittedCharacters.Clear();
            splitChannelParticleSystem.Stop();
        }
    }

    public void OnSwitchCharacter(InputValue value)
    {
        // We cannot switch just yet while we are still perform a jump
        if (verticalAcceleration.HasValue)
            return;

        ReleaseGrabbedObject();

        if (splittedCharacters.Count == 0)
            return;

        controlledSplittedCharacterIndex = (controlledSplittedCharacterIndex + 1) % splittedCharacters.Count;
        activePlayerCharacterController = splittedCharacters[controlledSplittedCharacterIndex].transform.GetComponent<CharacterController>();
        PlayerCameraFollower.Target = activePlayerCharacterController.transform;
    }

    public void OnJump(InputValue value)
    {
        // We cannot jump just yet again while we are still perform a jump
        if (verticalAcceleration.HasValue)
            return;

        if (!activePlayerCharacterController.transform.CompareTag("PlayerJump"))
            return;

        verticalAcceleration = Mathf.Sqrt(Gravity * MaxJumpHeight * 2);
    }

    public void OnGrab(InputValue value)
    {
        if (!activePlayerCharacterController.transform.CompareTag("PlayerGrab"))
            return;

        if (grabbedObject == null)
        {
            if (Physics.Raycast(activePlayerCharacterController.transform.position, activePlayerCharacterController.transform.TransformDirection(Vector3.forward), out RaycastHit hitInfo, 0.6f))
            {
                if (hitInfo.transform.CompareTag("MovableObject"))
                {
                    hitInfo.transform.SetParent(activePlayerCharacterController.transform);
                    grabbedObject = hitInfo.transform;
                    previousGrabObjectPosition = grabbedObject.position;
                }
            }
        }
        else
            ReleaseGrabbedObject();
    }

    public void ReleaseGrabbedObject()
    {
        if (grabbedObject == null)
            return;

        transform.position = previousPosition;
        grabbedObject.position = previousGrabObjectPosition;
        grabbedObject.SetParent(null);
        grabbedObject = null;
    }

    private void UpdateGrabCollision()
    {
        if (grabbedObject == null)
            return;

        int collisionCount = Physics.OverlapBoxNonAlloc(grabbedObject.position, grabbedObject.localScale / 2, colliderCache, grabbedObject.rotation);
        if (collisionCount == 0)
            return;

        for (int i = 0; i < collisionCount; ++i)
        {
            Collider collider = colliderCache[i];
            if (collider.transform == activePlayerCharacterController.transform 
                || collider.transform == grabbedObject.transform
                || collider.transform.IsChildOf(activePlayerCharacterController.transform))
                continue;

            ReleaseGrabbedObject();
            break;
        }
    }
}
