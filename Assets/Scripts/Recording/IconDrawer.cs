using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IconDrawer : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI 연결")]
    [SerializeField] private RawImage drawingCanvas; // 그림을 그릴 RawImage
    [SerializeField] private Button resetButton;    // 초기화 버튼
    [SerializeField] private Button saveButton;     // 적용 버튼
    [SerializeField] private Image previewThumbnail;
    [SerializeField] private GameObject popup;

    [Header("드로잉 설정")]
    [SerializeField] private int textureSize = 128;  // 텍스처 해상도 (픽셀 느낌을 위해 64~128 추천)
    [SerializeField] private int brushSize = 2;      // 붓 크기 (픽셀 단위)
    [SerializeField] private Color drawColor = Color.black; // 검은색 아이콘
    [SerializeField] private Color backgroundColor = Color.white; // 배경색

    private Texture2D drawableTexture;
    private RectTransform canvasRectTransform;


    private void Awake()
    {
        canvasRectTransform = drawingCanvas.GetComponent<RectTransform>();
        
        // 1. 텍스처 초기화 및 RawImage 연결
        InitTexture();

        // 2. 버튼 이벤트 연결
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetCanvas);
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveCanvas);
    }

    // 텍스처 생성 및 흰색 초기화
    private void InitTexture()
    {
        // ARGB32 포맷, FilterMode는 픽셀 아트를 위해 Point로 설정
        drawableTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        drawableTexture.filterMode = FilterMode.Point; 

        drawingCanvas.texture = drawableTexture;

        ResetCanvas();
    }

    // 캔버스 전체를 흰색(배경색)으로 초기화
    public void ResetCanvas()
    {
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }

        drawableTexture.SetPixels(pixels);
        drawableTexture.Apply(); // 텍스처 변경사항 적용
    }

    public void SaveCanvas()
    {
        Texture2D drawnTex = GetDrawnTexture();

        Sprite newIconSprite = Sprite.Create(
            drawnTex, 
            new Rect(0, 0, drawnTex.width, drawnTex.height), 
            new Vector2(0.5f, 0.5f)
        );

        // 스크립트 카드의 썸네일 Image 컴포넌트에 적용
        previewThumbnail.sprite = newIconSprite;

        // 팝업 닫기
        popup.SetActive(false);
    }
    // 포인터 클릭 시
    public void OnPointerDown(PointerEventData eventData)
    {
        DrawAtPointer(eventData);
    }

    // 포인터 드래그 시
    public void OnDrag(PointerEventData eventData)
    {
        DrawAtPointer(eventData);
    }

    // 마우스/터치 위치를 텍스처 픽셀 좌표로 변환하여 칠하기
    private void DrawAtPointer(PointerEventData eventData)
    {
        // RawImage의 로컬 좌표 구하기
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            // RectTransform 기준 (-Width/2 ~ Width/2) 좌표를 (0 ~ 1) UV 좌표로 변환
            Rect rect = canvasRectTransform.rect;
            float px = (localPoint.x - rect.x) / rect.width;
            float py = (localPoint.y - rect.y) / rect.height;

            // UV 좌표를 텍스처 픽셀 인덱스 좌표로 변환
            int texX = Mathf.FloorToInt(px * textureSize);
            int texY = Mathf.FloorToInt(py * textureSize);

            // 텍스처 범위 내에 있을 때 점 찍기
            if (texX >= 0 && texX < textureSize && texY >= 0 && texY < textureSize)
            {
                DrawBrush(texX, texY);
            }
        }
    }

    // 붓 크기(brushSize)만큼 픽셀 칠하기
    private void DrawBrush(int cx, int cy)
    {
        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int y = -brushSize; y <= brushSize; y++)
            {
                int targetX = cx + x;
                int targetY = cy + y;

                if (targetX >= 0 && targetX < textureSize && targetY >= 0 && targetY < textureSize)
                {
                    drawableTexture.SetPixel(targetX, targetY, drawColor);
                }
            }
        }
        
        // 변경된 픽셀들을 실제 텍스처에 반영
        drawableTexture.Apply();
    }

    // 작성한 텍스처를 반환 (스크립트 카드 썸네일 적용용)
    public Texture2D GetDrawnTexture()
    {
        return drawableTexture;
    }
}