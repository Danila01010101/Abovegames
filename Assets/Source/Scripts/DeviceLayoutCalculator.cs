using UnityEngine;

public class DeviceLayoutCalculator
{
    [SerializeField] private int phoneColumns = 2;
    [SerializeField] private int tabletColumns = 3;
    [SerializeField] private float spacing = 20f;
    
    private int currentColumns;
    private Vector2 currentCardSize;
    private bool isInitialized = false;
    
    public int GetCurrentColumns()
    {
        if (!isInitialized)
        {
            CalculateLayout();
        }
        return currentColumns;
    }
    
    public Vector2 GetCurrentCardSize(RectTransform container, float edgePadding)
    {
        if (!isInitialized)
        {
            CalculateLayout();
        }
        
        if (container == null) return currentCardSize;
        
        Vector2 availableSize = container.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);
        float totalSpacing = spacing * (currentColumns - 1);
        
        float availableWidth = availableSize.x - totalSpacing;
        float cellWidth = availableWidth / currentColumns;
        float cellHeight = cellWidth * 1.5f;
        
        currentCardSize = new Vector2(cellWidth, cellHeight);
        return currentCardSize;
    }
    
    private void CalculateLayout()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        bool isTablet = screenRatio < 1.7f && Screen.dpi < 400;
        
        currentColumns = isTablet ? tabletColumns : phoneColumns;
        isInitialized = true;
    }
}