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
    private Vector2Int? lastPixelPos = null;


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
        lastPixelPos = null;
    }

    public void SaveCanvas()
    {
        Texture2D drawnTex = GetDrawnTexture();

        // drawableTexture는 팝업을 다시 열 때마다 계속 재사용/수정되는 텍스처이므로
        // Sprite.Create에 그대로 넘기면 픽셀 데이터가 복사되지 않고 참조만 공유된다.
        // 그러면 이후에 다른 스크립트의 썸네일을 새로 그릴 때 이전에 저장해둔
        // 스프라이트까지 함께 바뀌어 버리므로, 저장 시점의 픽셀을 복제해 별도 텍스처로 만든다.
        Texture2D snapshot = new Texture2D(drawnTex.width, drawnTex.height, drawnTex.format, false);
        snapshot.filterMode = drawnTex.filterMode;
        snapshot.SetPixels(drawnTex.GetPixels());
        snapshot.Apply();

        Sprite newIconSprite = Sprite.Create(
            snapshot,
            new Rect(0, 0, snapshot.width, snapshot.height),
            new Vector2(0.5f, 0.5f)
        );

        ScriptWriteManager.instance.setPreviewThumbnail(newIconSprite);

        // 팝업 닫기
        popup.SetActive(false);
    }
    // 포인터 클릭 시
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2Int? currentPos = GetTexturePixelPos(eventData);
        if (currentPos.HasValue)
        {
            DrawBrush(currentPos.Value.x, currentPos.Value.y);
            drawableTexture.Apply();
            lastPixelPos = currentPos; // 첫 좌표 저장
        }
    }

    // 포인터 드래그 시
    public void OnDrag(PointerEventData eventData)
    {
        Vector2Int? currentPos = GetTexturePixelPos(eventData);

        if (currentPos.HasValue)
        {
            if (lastPixelPos.HasValue)
            {
                // 💡 이전 좌표와 현재 좌표 사이를 보간하여 선 긋기
                DrawLine(lastPixelPos.Value.x, lastPixelPos.Value.y, currentPos.Value.x, currentPos.Value.y);
            }
            else
            {
                DrawBrush(currentPos.Value.x, currentPos.Value.y);
            }

            drawableTexture.Apply(); // 변경사항 한 번에 반영
            lastPixelPos = currentPos; // 현재 좌표를 다음 프레임의 이전 좌표로 갱신
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        lastPixelPos = null; // 연결 초기화
    }
    private Vector2Int? GetTexturePixelPos(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Rect rect = canvasRectTransform.rect;
            float px = (localPoint.x - rect.x) / rect.width;
            float py = (localPoint.y - rect.y) / rect.height;

            int texX = Mathf.FloorToInt(px * textureSize);
            int texY = Mathf.FloorToInt(py * textureSize);

            if (texX >= 0 && texX < textureSize && texY >= 0 && texY < textureSize)
            {
                return new Vector2Int(texX, texY);
            }
        }
        return null;
    }
    private void DrawLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawBrush(x0, y0);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
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