using UnityEngine;

public class OptimizedVisibilityScript : MonoBehaviour
{
    private Camera mainCamera;
    private Renderer[] childRenderers;
    private bool[] wasVisible;

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

            bool isVisible = GeometryUtility.TestPlanesAABB(planes, childRenderers[i].bounds);

            if (childRenderers[i].gameObject.activeSelf != isVisible)
            {
                childRenderers[i].gameObject.SetActive(isVisible);
            }

            wasVisible[i] = isVisible;
        }
    }
}
