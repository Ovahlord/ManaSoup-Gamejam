using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterMovableObjectGlue : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (transform.parent == hit.transform)
            return;

        if (hit.transform.CompareTag("MovableObject"))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.5)
                transform.SetParent(hit.transform);
        }
        else
            transform.SetParent(null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.CompareTag("AmplifyJump"))
            return;

        PlayerCharacterController.ToggleAmplifiedJumping(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.CompareTag("AmplifyJump"))
            return;

        PlayerCharacterController.ToggleAmplifiedJumping(true);
    }
}
