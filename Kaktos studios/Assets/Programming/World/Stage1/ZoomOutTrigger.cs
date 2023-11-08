using UnityEngine;

public class ZoomOutTrigger : MonoBehaviour
{
    [SerializeField] private float targetFOV = 60.0f;
    [SerializeField] private float zoomOutDuration = 2.0f;

    [SerializeField] private float distanceChange = 0;

    public float TargetFOV { get { return targetFOV; } }
    public float TargetDuration { get { return zoomOutDuration; } }
    public float TargetDistance { get { return distanceChange; } }
}