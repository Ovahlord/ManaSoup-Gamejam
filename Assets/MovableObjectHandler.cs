using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObjectHandler : MonoBehaviour
{
    private Collider[] colliderCache = new Collider[100];
    private new BoxCollider collider = null;
    private Vector3 previousPosition = Vector3.zero;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (transform.parent == null || !transform.parent.CompareTag("PlayerGrab"))
            return;

        int collisions = Physics.OverlapBoxNonAlloc(gameObject.transform.position, transform.localScale / 2, colliderCache, transform.rotation);
        for (int i = 0; i < collisions; ++i)
        {
            Collider col = colliderCache[i];
            if (col.transform == transform || col.transform.IsChildOf(transform.parent))
                continue;

            PlayerCharacterController.ReleaseGrabbedObject();
            transform.position = previousPosition;
            break;
        }

        previousPosition = transform.position;
    }
}
