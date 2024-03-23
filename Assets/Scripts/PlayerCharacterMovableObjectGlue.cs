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

        PlayerCharacterController.ToggleAmplifiedJumping(hit.transform.CompareTag("AmplifyJump"));
    }
}
