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
    [SerializeField] private int requiredPickups = 3;
    [SerializeField] private Material mainCharacterMaterial = null;
    [SerializeField] private AudioClip jumpSoundEffect = null;
    [SerializeField] private Animator animator = null;

    [Header("Jump Character Settings")]
    [SerializeField] private float MaxJumpHeight = 5f;
    [SerializeField] private float Gravity = 40f;
    [SerializeField] private float AmplifiedJumpMultiplier = 1.5f;

    private CharacterController activePlayerCharacterController = null;
    private Vector2 moveInputValue = Vector2.zero;
    private Vector3 motion = Vector3.zero;
    private float targetBodyRotation = 0f;
    private List<GameObject> splittedCharacters = new List<GameObject>();
    private int controlledSplittedCharacterIndex = 0;
    private float? verticalAcceleration = null;
    private PlayerInput input = null;
    static PlayerCharacterController instance = null;

    // Grabbing
    private Transform grabbedObject = null;
    private Vector3 previousPosition = Vector3.zero;
    private Vector3 previousGrabObjectPosition = Vector3.zero;
    private Collider[] colliderCache = new Collider[20];

    // Amplified Jumping
    private bool amplifiedJump = false;

    // Pickups
    private int gatheredPickups = 0;
    public static int GatheredPickups 
    {
        get { return instance.gatheredPickups; }
        private set
        {
            instance.gatheredPickups = value;
            PlayerHUDManager.SetPickupValues(instance.requiredPickups, value);
        }
    }

    public static int RequiredPickups
    {
        get { return instance.requiredPickups; }
    }

    private readonly Vector3[] splittingRaycastDirections = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back, new(1f, 0f, 1f), new(-1f, 0f, -1f), new(1f, 0f, -1f), new(-1f, 0f, 1f) };

    private void Awake()
    {
        activePlayerCharacterController = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        new GameObject("CameraFollower").AddComponent<PlayerCameraFollower>();

        PlayerCameraFollower.Target = transform;
        PlayerCameraFollower.CameraDistance = cameraDistance;
        PlayerCameraFollower.FollowLerpFactor = cameraFollowLerpFactor;
        PlayerCameraFollower.CameraFacingHeightOffset = cameraFacingHeightOffset;

        previousPosition = activePlayerCharacterController.transform.position;
        if (instance != null && instance != this)
            Destroy(instance);

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Instantiate(playerHudPrefab);
        PlayerHUDManager.SetPickupValues(requiredPickups, GatheredPickups);
    }

    // Update is called once per frame
    void Update()
    {
        if (grabbedObject == null)
            activePlayerCharacterController.transform.rotation = Quaternion.Euler(0f, Mathf.MoveTowardsAngle(activePlayerCharacterController.transform.eulerAngles.y, targetBodyRotation, bodyTurnSpeed * Time.deltaTime), 0f);

        if (moveInputValue.magnitude == 0f && !verticalAcceleration.HasValue)
        {
            animator.SetTrigger("StopWalk");
            return;
        }

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
        else
        {
            if (!verticalAcceleration.HasValue)
                verticalAcceleration = 0f;

            ToggleAmplifiedJumping(false);
        }

        animator.SetTrigger("StartWalk");
        UpdateGrabCollision();
    }

    void LateUpdate()
    {
        UpdatePositionSnapshots();
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

            mainCharacterMaterial.SetFloat("_TransparencyModifier", 0.2f);
        }
        else
        {
            activePlayerCharacterController = GetComponent<CharacterController>();
            PlayerCameraFollower.Target = transform;

            ReleaseGrabbedObject();

            foreach (var splittedCharacter in splittedCharacters)
                Destroy(splittedCharacter);

            splittedCharacters.Clear();
            mainCharacterMaterial.SetFloat("_TransparencyModifier", 1.0f);
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
        if (amplifiedJump && activePlayerCharacterController.transform.CompareTag("PlayerJump"))
            verticalAcceleration *= AmplifiedJumpMultiplier;

        AudioSource.PlayClipAtPoint(jumpSoundEffect, activePlayerCharacterController.transform.position, 10f);
    }

    public void OnGrab(InputValue value)
    {
        if (!activePlayerCharacterController.transform.CompareTag("PlayerGrab"))
            return;

        if (grabbedObject == null)
        {
            if (Physics.Raycast(activePlayerCharacterController.transform.position + Vector3.up * 0.4f, activePlayerCharacterController.transform.TransformDirection(Vector3.forward), out RaycastHit hitInfo, 0.6f))
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

    public void OnOpenMenu(InputValue value)
    {
        PlayerHUDManager.ToggleMenu(true);
        input.SwitchCurrentActionMap("UI");
        Cursor.visible = true;
    }

    public void OnCloseMenu(InputValue value)
    {
        PlayerHUDManager.ToggleMenu(false);
        input.SwitchCurrentActionMap("Player");
        Cursor.visible = false;
    }

    public void ReleaseGrabbedObject()
    {
        if (grabbedObject == null)
            return;

        grabbedObject.SetParent(null);
        grabbedObject.position = previousGrabObjectPosition;
        grabbedObject = null;

        activePlayerCharacterController.Move(previousPosition - activePlayerCharacterController.transform.position);
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
                || collider.transform.IsChildOf(activePlayerCharacterController.transform)
                || collider.isTrigger)
                continue;

            ReleaseGrabbedObject();
            break;
        }
    }

    private void UpdatePositionSnapshots()
    {
        previousPosition = activePlayerCharacterController.transform.position;
        if (grabbedObject != null)
            previousGrabObjectPosition = grabbedObject.position;
    }

    public static void ToggleAmplifiedJumping(bool enable)
    {
        if (instance.amplifiedJump == enable || !instance.activePlayerCharacterController.transform.CompareTag("PlayerJump"))
            return;

        instance.amplifiedJump = enable;
        if (instance.activePlayerCharacterController.TryGetComponent(out ParticleSystem particles))
        {
            if (enable)
                particles.Play();
            else
                particles.Stop();
        }

    }

    public static void IncrementPickupNumber()
    {
        ++GatheredPickups;
    }
}
