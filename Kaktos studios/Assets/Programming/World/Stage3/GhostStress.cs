using System.Collections;
using UnityEngine;

public class GhostStress : MonoBehaviour
{
    public float transparencyDistance = 5f;
    public float stressIncreaseAmount = 10f;
    public PlayerController playerController; // Assign this via the inspector

    private Material objectMaterial;
    private Color originalColor;

    void Start()
    {
        objectMaterial = GetComponentInChildren<Renderer>().material;
        originalColor = objectMaterial.color;
        SetTransparency(0f); // Start fully transparent
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, playerController.transform.position) < transparencyDistance)
        {
            SetTransparency(1f); // Fully opaque
            playerController.IncreaseStress(stressIncreaseAmount);
        }
        else
        {
            SetTransparency(0f); // Fully transparent
        }
    }

    void SetTransparency(float alpha)
    {
        Color newColor = originalColor;
        newColor.a = alpha;
        objectMaterial.color = newColor;
    }
}
