using UnityEngine;

public class ResolutionCameraRatio : MonoBehaviour
{
    private void Awake()
    {
        Camera camera = GetComponent<Camera>();
        Rect rect = camera.rect;

        // 목표 화면비 (16:9)
        float targetAspect = 1920f / 1080f;
        // 현재 윈도우 창/모니터 화면비
        float currentAspect = (float)Screen.width / Screen.height;

        float scaleHeight = currentAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // 위아래 레터박스 추가
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // 양옆 필러박스(검은 여백) 추가
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        camera.rect = rect;
    }
}