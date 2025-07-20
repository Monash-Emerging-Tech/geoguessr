
using UnityEngine;

public class Expand_Close_Minimap : MonoBehaviour
{
    public RectTransform mapContainer; 
    public RectTransform primaryButton; 
    public RectTransform cornerButtons;
    public Vector3 minimizedScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 expandedScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float scaleSpeed = 5f;

    private Vector3 targetScale;
    private float targetWidth;

    void Start()
    {
        // Initialise in expanded view
        targetScale = expandedScale;
        mapContainer.localScale = targetScale;
        if (mapContainer != null)
            targetWidth = mapContainer.rect.width * expandedScale.x;
        if (primaryButton != null)
            primaryButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        // Ensure cornerButtons are visible and sized in expanded view
        if (cornerButtons != null)
        {
            float expandedWidth = cornerButtons.rect.width;
            float expandedHeight = cornerButtons.rect.height;
            cornerButtons.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, expandedWidth);
            cornerButtons.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, expandedHeight);
            // Position will be handled in Update()
        }
    }

    void Update()
    {
        if (mapContainer.localScale != targetScale)
        {
            mapContainer.localScale = Vector3.Lerp(mapContainer.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
        if (primaryButton != null && mapContainer != null)
        {
            float currentWidth = primaryButton.rect.width;
            if (Mathf.Abs(currentWidth - targetWidth) > 0.01f)
            {
                float newWidth = Mathf.Lerp(currentWidth, targetWidth, Time.deltaTime * scaleSpeed);
                primaryButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            }
            // Align right edge every frame
            float scaleX = mapContainer.localScale.x;
            float mapRight = mapContainer.anchoredPosition.x + (mapContainer.rect.width * (1 - mapContainer.pivot.x)) * scaleX;
            float buttonRight = primaryButton.anchoredPosition.x + (primaryButton.rect.width * (1 - primaryButton.pivot.x));
            float offset = mapRight - buttonRight;
            primaryButton.anchoredPosition = new Vector2(primaryButton.anchoredPosition.x + offset, primaryButton.anchoredPosition.y);
        }

        // Position cornerButtons: left edge matches mapContainer's left edge, bottom edge touches mapContainer's top edge, size stays consistent
        if (cornerButtons != null && mapContainer != null)
        {
            // Use expanded size for cornerButtons
            float expandedWidth = cornerButtons.rect.width;
            float expandedHeight = cornerButtons.rect.height;
            cornerButtons.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, expandedWidth);
            cornerButtons.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, expandedHeight);

            float scaleX = mapContainer.localScale.x;
            float scaleY = mapContainer.localScale.y;
            // MapContainer left edge (in parent space)
            float mapLeft = mapContainer.anchoredPosition.x - (mapContainer.rect.width * mapContainer.pivot.x) * scaleX;
            // MapContainer top edge (in parent space)
            float mapTop = mapContainer.anchoredPosition.y + (mapContainer.rect.height * (1 - mapContainer.pivot.y)) * scaleY;
            // CornerButtons pivot
            float cornerPivotX = cornerButtons.pivot.x;
            float cornerPivotY = cornerButtons.pivot.y;
            // Set anchoredPosition so left edge matches mapLeft, bottom edge matches mapTop
            float newX = mapLeft + expandedWidth * cornerPivotX;
            float newY = mapTop + expandedHeight * cornerPivotY;
            // The bottom edge of cornerButtons (anchoredPosition.y - expandedHeight * cornerPivotY) will match mapTop
            cornerButtons.anchoredPosition = new Vector2(newX, newY);
        }
    }

    public void Minimize()
    {
        targetScale = minimizedScale;
        if (mapContainer != null)
            targetWidth = mapContainer.rect.width * minimizedScale.x;
        // Align right edge after width is set
        if (primaryButton != null && mapContainer != null)
        {
            float mapRight = mapContainer.anchoredPosition.x + (mapContainer.rect.width * (1 - mapContainer.pivot.x)) * minimizedScale.x;
            float buttonRight = primaryButton.anchoredPosition.x + (primaryButton.rect.width * (1 - primaryButton.pivot.x)) * minimizedScale.x;
            float offset = mapRight - buttonRight;
            primaryButton.anchoredPosition = new Vector2(primaryButton.anchoredPosition.x + offset, primaryButton.anchoredPosition.y);
        }
    }

    public void Expand()
    {
        targetScale = expandedScale;
        if (mapContainer != null)
            targetWidth = mapContainer.rect.width * expandedScale.x;
        // Align right edge after width is set
        if (primaryButton != null && mapContainer != null)
        {
            float mapRight = mapContainer.anchoredPosition.x + (mapContainer.rect.width * (1 - mapContainer.pivot.x)) * expandedScale.x;
            float buttonRight = primaryButton.anchoredPosition.x + (primaryButton.rect.width * (1 - primaryButton.pivot.x)) * expandedScale.x;
            float offset = mapRight - buttonRight;
            primaryButton.anchoredPosition = new Vector2(primaryButton.anchoredPosition.x + offset, primaryButton.anchoredPosition.y);
        }
    }
}
