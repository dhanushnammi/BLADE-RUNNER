using UnityEngine;
public class NeonCameraFollow : MonoBehaviour
{
    public Transform target;
    void LateUpdate() { if (target) transform.position = target.position; }
}
