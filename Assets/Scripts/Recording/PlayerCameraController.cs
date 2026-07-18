using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothing = 0.2f;
    [SerializeField] private Vector2 minCameraBoundary;
    [SerializeField] private Vector2 maxCameraBoundary;
    private void FixedUpdate()
    {
        Vector3 targetPos = new Vector3(playerTransform.position.x, playerTransform.position.y, this.transform.position.z);
        targetPos.x = Mathf.Clamp(targetPos.x, minCameraBoundary.x, maxCameraBoundary.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minCameraBoundary.y, maxCameraBoundary.y);

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
    }
}
