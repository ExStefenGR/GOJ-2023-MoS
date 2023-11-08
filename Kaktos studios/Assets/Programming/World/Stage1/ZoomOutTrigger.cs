using UnityEngine;

public class ZoomOutTrigger : MonoBehaviour
{
    [SerializeField] private float targetFOV = 60.0f;
    [SerializeField] private float zoomOutDuration = 2.0f;
    [SerializeField] private float distanceChange = 0;
    [SerializeField] private bool isTriggered = false;

    public float TargetFOV { get { return targetFOV; } }
    public float TargetDuration { get { return zoomOutDuration; } }
    public float TargetDistance { get { return distanceChange; } }

    // Property to encapsulate the isTriggered state
    public bool IsTriggered
    {
        get => isTriggered;
        private set => isTriggered = value;
    }

    // Call this to activate the zoom, if it has not already been triggered
    public bool ActivateZoom()
    {
        if (!IsTriggered)
        {
            IsTriggered = true;
            return true;
        }
        return false;
    }
}