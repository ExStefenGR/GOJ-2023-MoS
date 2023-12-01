using UnityEngine;

public class OptimizedVisibilityScript : MonoBehaviour
{
    private Camera mainCamera;
    private Renderer[] childRenderers;
    private bool[] wasVisible;
    public float visibilityBuffer = 1.0f; // Buffer for visibility check

    void Start()
    {
        mainCamera = Camera.main;
        childRenderers = GetComponentsInChildren<Renderer>();
        wasVisible = new bool[childRenderers.Length];
    }

    void Update()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] == null)
                continue;

            // Create a slightly larger bounds for the visibility check
            Bounds expandedBounds = childRenderers[i].bounds;
            expandedBounds.Expand(visibilityBuffer);

            bool isVisible = GeometryUtility.TestPlanesAABB(planes, expandedBounds);

            if (childRenderers[i].gameObject.activeSelf != isVisible)
            {
                childRenderers[i].gameObject.SetActive(isVisible);
            }

            wasVisible[i] = isVisible;
        }
    }
}
