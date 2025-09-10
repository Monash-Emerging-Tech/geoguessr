
using UnityEngine;
using UnityEngine.UI;


/// Expand/Close Minimap
/// 
/// Written by Ange
/// 
/// </summary>

public class Expand_Close_Minimap : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform mapContainer; 
    public RectTransform primaryButton; 
    public RectTransform cornerButtons;
    public Vector3 minimizedScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 expandedScale = new Vector3(5f, 5f, 5f);
    public float scaleSpeed = 5f;

    public bool fullscreen;

    private Vector3 targetScale;
    private float targetWidth;

    private GameLogic gameLoader;
    void Start()
    {
        // Initialise in expanded view
        targetScale = minimizedScale;
        mapContainer.localScale = targetScale;
        if (mapContainer != null)
            targetWidth = mapContainer.rect.width * targetScale.x;
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

        gameLoader = GameObject.Find("GameLogic").GetComponent<GameLogic>();
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

    public void Expand(bool fullscreen = false)
    {
        targetScale = !fullscreen? expandedScale : getFullScreenScale();

        if (mapContainer != null)
            targetWidth = mapContainer.rect.width * targetScale.x;
        // Align right edge after width is set
        if (primaryButton != null && mapContainer != null)
        {
            float mapRight = mapContainer.anchoredPosition.x + (mapContainer.rect.width * (1 - mapContainer.pivot.x)) * targetScale.x;
            float buttonRight = primaryButton.anchoredPosition.x + (primaryButton.rect.width * (1 - primaryButton.pivot.x)) * targetScale.x;
            float offset = mapRight - buttonRight;
            primaryButton.anchoredPosition = new Vector2(primaryButton.anchoredPosition.x + offset, primaryButton.anchoredPosition.y);
        }
    }

    // Uses the Canvas Default Resolution Size to find how much to scale the map to fullscreen after the guess button is made
    // Default Resolution at the time of writing this is 1920x1080
    public Vector3 getFullScreenScale()
    {
        
        RectTransform rectTransform = mapContainer.GetComponent<RectTransform>();
        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();

        //Debug.Log(Screen.width);
        //Debug.Log(rectTransform.rect.width);

        Vector3 fullScreenScale = new Vector3(
                ((canvasScaler.referenceResolution.x - 30) / rectTransform.rect.width),
                (canvasScaler.referenceResolution.y / rectTransform.rect.height) * 80/100,
                ((canvasScaler.referenceResolution.y / rectTransform.rect.height) * 80/100));


        //Debug.Log($"{fullScreenScale}");

        return fullScreenScale;
    }


    public void GuessButtonClick()
    {

        if (gameLoader.isGuessing)
        {
            Expand(true);
        }
        else
        {
            Minimize();
        }

    }
}
