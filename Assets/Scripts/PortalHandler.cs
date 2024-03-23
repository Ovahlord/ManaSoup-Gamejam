using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> portalFragments = new List<GameObject>();
    [SerializeField] private List<Material> portalFragmentMaterials = new List<Material>();
    [SerializeField] private Transform portalVisual = null;

    private List<GameObject> missingFragments = new List<GameObject>();
    private float? portalGrowthTimer = null;
    private bool toggledEmission = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Material mat in portalFragmentMaterials)
            mat.DisableKeyword("_EMISSION");

        int removedFragments = Mathf.Min(PlayerCharacterController.RequiredPickups, portalFragments.Count);
        if (removedFragments == 0)
            return;

        portalVisual.localScale = Vector3.zero;

        for (int i = 0; i < removedFragments; ++i)
        {
            portalFragments[i].transform.localScale = Vector3.zero;
            missingFragments.Add(portalFragments[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!portalGrowthTimer.HasValue)
            return;

        if (portalGrowthTimer >= Time.deltaTime)
            portalGrowthTimer -= Time.deltaTime;

        if (portalGrowthTimer <= Time.deltaTime)
        {
            portalVisual.localScale = Vector3.MoveTowards(portalVisual.localScale, Vector3.one * 0.5f, Time.deltaTime / 3);

            if (!toggledEmission)
            {
                toggledEmission = true;
                foreach (Material mat in portalFragmentMaterials)
                    mat.EnableKeyword("_EMISSION");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.CompareTag("Player"))
            return;

        int pickupDelta = PlayerCharacterController.RequiredPickups - PlayerCharacterController.GatheredPickups;
        while (missingFragments.Count > pickupDelta)
        {
            missingFragments[0].transform.localPosition = missingFragments[0].transform.position - (other.transform.position + Vector3.up * 0.5f);
            missingFragments[0].AddComponent<PortalFragmentMovement>();
            missingFragments.Remove(missingFragments[0]);
        }

        if (missingFragments.Count == 0 && !portalGrowthTimer.HasValue)
            portalGrowthTimer = 2f;
    }
}
