using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // 메인 카메라 Transform
    [SerializeField] private float parallaxFactor;      // 패럴랙스 비율 (전경: 1.3 / 크로마키: 0.6 / 원경: 0.2 등)

    private Vector3 lastCameraPosition;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        // 지난 프레임 대비 카메라가 이동한 거리 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 비율을 곱하여 레이어 이동 (X축 횡스크롤 기준)
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, deltaMovement.y * parallaxFactor, 0);

        lastCameraPosition = cameraTransform.position;
    }
}
