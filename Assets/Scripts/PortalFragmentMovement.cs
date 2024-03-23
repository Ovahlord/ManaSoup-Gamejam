using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalFragmentMovement : MonoBehaviour
{
    private float distPerSecond = 0f;
    private float movementVariance = 1f;
    private float scaleVariance = 1f;

    // Start is called before the first frame update
    void Start()
    {
        distPerSecond = transform.localPosition.magnitude;
        movementVariance = Random.Range(0.5f, 1f);
        scaleVariance = Random.Range(0.5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, distPerSecond * Time.deltaTime * movementVariance);
        transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * scaleVariance);
    }
}
