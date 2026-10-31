
using UnityEngine;

public class MainGameHUDAligner : MonoBehaviour
{
    [Header("Arka Plan")]
    public SpriteRenderer mineBackground;
    public Camera gameCamera;

    [Header("Altin ve Elmas")]
    public RectTransform goldText;
    public RectTransform diamondText;

    [Header("Enerji")]
    public RectTransform energyFill;
    public RectTransform energyText;

    [Header("Canta")]
    public RectTransform bagFill;
    public RectTransform bagText;

    private RectTransform hudRect;
    private Canvas canvas;

    private void Awake()
    {
        hudRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void LateUpdate()
    {
        if (mineBackground == null ||
            mineBackground.sprite == null ||
            canvas == null ||
            hudRect == null)
            return;

        Camera cam = gameCamera != null
            ? gameCamera
            : Camera.main;

        if (cam == null)
            return;

        Camera uiCamera =
            canvas.rootCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.rootCanvas.worldCamera;

        // Konumlar orijinal 941x1672 PNG
        // uzerindeki siyah alanlara gore.

        // ALTIN
        Align(
            goldText, 176f, 94f,
            108f, 38f, cam, uiCamera
        );

        // ELMAS
        Align(
            diamondText, 393f, 94f,
            108f, 38f, cam, uiCamera
        );

        // ENERJI
        Align(
            energyFill, 616f, 94f,
            124f, 28f, cam, uiCamera
        );

        Align(
            energyText, 616f, 94f,
            124f, 38f, cam, uiCamera
        );

        // CANTA
        Align(
            bagFill, 847f, 94f,
            112f, 28f, cam, uiCamera
        );

        Align(
            bagText, 847f, 94f,
            112f, 38f, cam, uiCamera
        );
    }

    private Vector2 ImageToUI(
        float pixelX,
        float pixelY,
        Camera cam,
        Camera uiCamera)
    {
        Sprite sprite = mineBackground.sprite;
        Bounds bounds = sprite.bounds;

        float normalizedX =
            pixelX / sprite.rect.width;

        float normalizedY =
            pixelY / sprite.rect.height;

        Vector3 local = new Vector3(
            bounds.min.x +
            bounds.size.x * normalizedX,

            bounds.max.y -
            bounds.size.y * normalizedY,

            0f
        );

        Vector3 world =
            mineBackground.transform.TransformPoint(
                local
            );

        Vector3 screen =
            cam.WorldToScreenPoint(world);

        Vector2 result;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                hudRect,
                new Vector2(screen.x, screen.y),
                uiCamera,
                out result
            );

        return result;
    }

    private void Align(
        RectTransform element,
        float x,
        float y,
        float width,
        float height,
        Camera cam,
        Camera uiCamera)
    {
        if (element == null)
            return;

        Vector2 center =
            ImageToUI(x, y, cam, uiCamera);

        Vector2 left =
            ImageToUI(
                x - width / 2f,
                y,
                cam,
                uiCamera
            );

        Vector2 right =
            ImageToUI(
                x + width / 2f,
                y,
                cam,
                uiCamera
            );

        Vector2 top =
            ImageToUI(
                x,
                y - height / 2f,
                cam,
                uiCamera
            );

        Vector2 bottom =
            ImageToUI(
                x,
                y + height / 2f,
                cam,
                uiCamera
            );

        element.anchorMin =
            new Vector2(0.5f, 0.5f);

        element.anchorMax =
            new Vector2(0.5f, 0.5f);

        element.pivot =
            new Vector2(0.5f, 0.5f);

        element.anchoredPosition = center;

        element.sizeDelta = new Vector2(
            Mathf.Abs(right.x - left.x),
            Mathf.Abs(top.y - bottom.y)
        );
    }
}
